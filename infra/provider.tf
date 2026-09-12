provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project     = var.project_name
      Environment = var.environment
      ManagedBy   = "Terraform"
      Component   = "autenticacao-lambda"
    }
  }
}

data "terraform_remote_state" "database" {
  backend = "s3"

  config = {
    bucket       = var.state_bucket_name
    key          = "oficina-mecanica/database/terraform.tfstate"
    region       = var.aws_region
    use_lockfile = true
  }
}

data "terraform_remote_state" "application" {
  backend = "s3"

  config = {
    bucket       = var.state_bucket_name
    key          = "oficina-mecanica/application/terraform.tfstate"
    region       = var.aws_region
    use_lockfile = true
  }
}

data "aws_iam_role" "lab_role" {
  name = "LabRole"
}
