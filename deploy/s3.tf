resource "aws_s3_bucket" "polly_universe" {
  bucket = local.app_name
}

resource "aws_s3_bucket_versioning" "polly_universe" {
  bucket = aws_s3_bucket.polly_universe.id
  versioning_configuration {
    status = "Enabled"
  }
}
