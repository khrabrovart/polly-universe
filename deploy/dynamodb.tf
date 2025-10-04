resource "aws_dynamodb_table" "voting_profiles" {
  name         = "${local.app_name}-voting-profiles"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S"
  }
}

resource "aws_dynamodb_table" "telegram_client_data" {
  name         = "${local.app_name}-telegram-client-data"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S"
  }
}
