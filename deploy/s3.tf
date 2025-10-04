resource "aws_s3_bucket" "polly_universe" {
  bucket = "polly-universe"
}

resource "aws_s3_bucket_versioning" "polly_universe" {
  bucket = aws_s3_bucket.polly_universe.id
  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_encryption" "polly_universe" {
  bucket = aws_s3_bucket.polly_universe.id

  server_side_encryption_configuration {
    rule {
      apply_server_side_encryption_by_default {
        sse_algorithm = "AES256"
      }
    }
  }
}

resource "aws_s3_bucket_public_access_block" "polly_universe" {
  bucket = aws_s3_bucket.polly_universe.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}
