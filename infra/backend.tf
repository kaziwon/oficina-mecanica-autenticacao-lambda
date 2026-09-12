terraform {
  backend "s3" {
    key          = "oficina-mecanica/auth-lambda/terraform.tfstate"
    region       = "us-east-1"
    encrypt      = true
    use_lockfile = true
  }
}
