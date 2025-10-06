variable "aws_region" {
  description = "The AWS region to deploy resources in"
  type        = string
}

variable "poll_waiting_minutes" {
  description = "The number of minutes to wait for poll to appear"
  type        = number
}

variable "openai_model" {
  description = "The OpenAI model to use for text generation"
  type        = string
}
