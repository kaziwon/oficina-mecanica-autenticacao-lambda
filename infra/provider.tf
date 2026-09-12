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

data "terraform_remote_state" "platform" {
  backend = "s3"

  config = {
    bucket  = var.state_bucket_name
    key     = "oficina-mecanica/platform/terraform.tfstate"
    region  = var.aws_region
    encrypt = true
  }
}

data "terraform_remote_state" "workloads" {
  backend = "s3"

  config = {
    bucket  = var.state_bucket_name
    key     = "oficina-mecanica/workloads/terraform.tfstate"
    region  = var.aws_region
    encrypt = true
  }
}

data "aws_iam_role" "lab_role" {
  name = "LabRole"
}
