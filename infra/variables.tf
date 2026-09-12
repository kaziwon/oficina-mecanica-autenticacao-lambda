variable "aws_region" {
  description = "Regiao usada pelo AWS Academy."
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Nome base dos recursos."
  type        = string
  default     = "oficina-mecanica"
}

variable "environment" {
  description = "Ambiente da funcao."
  type        = string
  default     = "production"
}

variable "state_bucket_name" {
  description = "Bucket S3 que contem os estados da plataforma e da aplicacao."
  type        = string

  validation {
    condition     = length(trimspace(var.state_bucket_name)) > 0
    error_message = "O nome do bucket de estado nao pode ser vazio."
  }
}

variable "lambda_memory_size" {
  description = "Memoria reservada para a Lambda em MB."
  type        = number
  default     = 256
}

variable "lambda_timeout" {
  description = "Tempo limite de cada autenticacao em segundos."
  type        = number
  default     = 15
}

variable "token_expiration_minutes" {
  description = "Validade do JWT emitido para o cliente."
  type        = number
  default     = 120
}
