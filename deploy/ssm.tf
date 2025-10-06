resource "aws_ssm_parameter" "bot_token" {
  name  = "/polly-universe/bot-token"
  type  = "SecureString"
  value = "placeholder"

  lifecycle {
    ignore_changes = [value]
  }
}

resource "aws_ssm_parameter" "openai_api_key" {
  name  = "/polly-universe/openai-api-key"
  type  = "SecureString"
  value = "placeholder"

  lifecycle {
    ignore_changes = [value]
  }
}

resource "aws_ssm_parameter" "notifications_peer_id" {
  name  = "/polly-universe/notifications-peer-id"
  type  = "String"
  value = "placeholder"

  lifecycle {
    ignore_changes = [value]
  }
}
