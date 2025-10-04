terraform {
  required_version = ">= 1.13"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.15"
    }
  }

  backend "s3" {
    bucket  = "arturkhrabrov-tfstate"
    key     = "polly-universe/terraform.tfstate"
    region  = "us-east-1"
    encrypt = true
  }
}

provider "aws" {
  region = "us-east-1"

  default_tags {
    tags = {
      Project   = "PollyUniverse"
      ManagedBy = "Terraform"
    }
  }
}
