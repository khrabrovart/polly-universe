resource "aws_dynamodb_table" "voting_profiles" {
  name             = "${local.app_name}-voting-profiles"
  billing_mode     = "PAY_PER_REQUEST"
  hash_key         = "Id"
  stream_enabled   = true
  stream_view_type = "NEW_AND_OLD_IMAGES"

  attribute {
    name = "Id"
    type = "S"
  }
}

resource "aws_dynamodb_table" "users" {
  name         = "${local.app_name}-users"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S"
  }
}
