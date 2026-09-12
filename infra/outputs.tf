output "function_name" {
  description = "Nome da Function Serverless."
  value       = aws_lambda_function.authentication.function_name
}

output "authentication_url" {
  description = "Endpoint HTTPS publico para autenticacao por CPF."
  value       = aws_lambda_function_url.authentication.function_url
}

output "cloudwatch_log_group" {
  description = "Grupo que recebe os logs estruturados da funcao."
  value       = aws_cloudwatch_log_group.lambda.name
}
