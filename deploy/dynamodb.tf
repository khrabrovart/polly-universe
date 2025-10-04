resource "aws_dynamodb_table" "voting_profiles" {
  name         = "${local.app_name}-voting-profiles"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S"
  }
}

resource "aws_dynamodb_table" "session_metadata" {
  name         = "${local.app_name}-session-metadata"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S"
  }
}
