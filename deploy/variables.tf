variable "aws_region" {
  description = "The AWS region to deploy resources in"
  type        = string
  default     = "us-east-1"
}

variable "poll_waiting_minutes" {
  description = "The number of minutes to wait for poll to appear"
  type        = number
  default     = 2
}
