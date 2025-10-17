locals {
  app_name = "polly-universe"

  shared_lambda_environment_vars = {
    S3_BUCKET             = aws_s3_bucket.polly_universe.bucket
    USERS_TABLE           = aws_dynamodb_table.users.name
    VOTING_PROFILES_TABLE = aws_dynamodb_table.voting_profiles.name
  }
}
