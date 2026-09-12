locals {
  function_name = "${var.project_name}-autenticacao-cpf"
  artifact_dir  = "${path.module}/../artifacts/lambda"

  database_host              = data.terraform_remote_state.platform.outputs.database_address
  database_port              = tostring(data.terraform_remote_state.platform.outputs.database_port)
  database_name              = data.terraform_remote_state.platform.outputs.database_name
  database_username          = data.terraform_remote_state.platform.outputs.database_username
  database_password          = data.terraform_remote_state.platform.outputs.database_password
  database_subnet_ids        = data.terraform_remote_state.platform.outputs.database_subnet_ids
  database_security_group_id = data.terraform_remote_state.platform.outputs.database_security_group_id
  jwt_secret                 = data.terraform_remote_state.workloads.outputs.jwt_secret
}

data "archive_file" "lambda" {
  type        = "zip"
  source_dir  = local.artifact_dir
  output_path = "${path.module}/autenticacao-lambda.zip"
}

resource "aws_security_group" "lambda" {
  name        = "${local.function_name}-sg"
  description = "Saida da Lambda de autenticacao para o RDS"
  vpc_id      = data.terraform_remote_state.platform.outputs.vpc_id

  tags = {
    Name = "${local.function_name}-sg"
  }
}

resource "aws_vpc_security_group_egress_rule" "lambda" {
  security_group_id = aws_security_group.lambda.id
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "-1"
}

resource "aws_vpc_security_group_ingress_rule" "database_from_lambda" {
  security_group_id            = local.database_security_group_id
  referenced_security_group_id = aws_security_group.lambda.id
  description                  = "MySQL a partir da Lambda de autenticacao"
  from_port                    = 3306
  to_port                      = 3306
  ip_protocol                  = "tcp"
}

resource "aws_cloudwatch_log_group" "lambda" {
  name              = "/aws/lambda/${local.function_name}"
  retention_in_days = 7
}

resource "aws_lambda_function" "authentication" {
  function_name = local.function_name
  description   = "Valida CPF, consulta o cliente no RDS e emite JWT."
  role          = data.aws_iam_role.lab_role.arn

  runtime       = "dotnet10"
  architectures = ["x86_64"]
  handler       = "OficinaMecanica.Auth.Function::OficinaMecanica.Auth.Function.Function::FunctionHandler"

  filename         = data.archive_file.lambda.output_path
  source_code_hash = data.archive_file.lambda.output_base64sha256

  memory_size                    = var.lambda_memory_size
  timeout                        = var.lambda_timeout
  reserved_concurrent_executions = 5

  vpc_config {
    subnet_ids         = local.database_subnet_ids
    security_group_ids = [aws_security_group.lambda.id]
  }

  environment {
    variables = {
      DATABASE_HOST            = local.database_host
      DATABASE_PORT            = local.database_port
      DATABASE_NAME            = local.database_name
      DATABASE_USER            = local.database_username
      DATABASE_PASSWORD        = local.database_password
      JWT_ISSUER               = "OficinaMecanica.Api"
      JWT_AUDIENCE             = "OficinaMecanica.Api"
      JWT_SECRET               = local.jwt_secret
      TOKEN_EXPIRATION_MINUTES = tostring(var.token_expiration_minutes)
    }
  }

  depends_on = [
    aws_cloudwatch_log_group.lambda,
    aws_vpc_security_group_ingress_rule.database_from_lambda,
  ]
}

resource "aws_lambda_function_url" "authentication" {
  function_name      = aws_lambda_function.authentication.function_name
  authorization_type = "NONE"
  invoke_mode        = "BUFFERED"

  cors {
    allow_origins = ["*"]
    allow_methods = ["POST"]
    allow_headers = ["content-type"]
    max_age       = 3600
  }
}
