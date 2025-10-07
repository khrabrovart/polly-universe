data "archive_file" "voting_lambda_zip" {
  type        = "zip"
  source_dir  = "../voting-lambda"
  output_path = "voting-lambda.zip"
}

resource "aws_lambda_function" "voting_lambda" {
  filename      = data.archive_file.voting_lambda_zip.output_path
  function_name = "${local.app_name}-voting"
  role          = aws_iam_role.voting_lambda_role.arn
  handler       = "PollyUniverse.Voting.Func::PollyUniverse.Voting.Func.Function::HandleEvent"
  runtime       = "dotnet8"
  timeout       = 300
  memory_size   = 512
  architectures = ["arm64"]

  source_code_hash = data.archive_file.voting_lambda_zip.output_base64sha256

  environment {
    variables = {
      SESSION_METADATA_TABLE          = aws_dynamodb_table.session_metadata.name
      VOTING_PROFILES_TABLE           = aws_dynamodb_table.voting_profiles.name
      S3_BUCKET                       = aws_s3_bucket.polly_universe.bucket
      POLL_WAITING_MINUTES            = var.poll_waiting_minutes
      BOT_TOKEN_PARAMETER             = aws_ssm_parameter.bot_token.name
      NOTIFICATIONS_PEER_ID_PARAMETER = aws_ssm_parameter.notifications_peer_id.name
      OPENAI_API_KEY_PARAMETER        = aws_ssm_parameter.openai_api_key.name
      OPENAI_MODEL                    = var.openai_model
    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.voting_lambda_basic_execution,
    aws_iam_role_policy_attachment.voting_lambda_policy_attachment,
    aws_cloudwatch_log_group.voting_lambda_logs
  ]
}

resource "aws_iam_role" "voting_lambda_role" {
  name = "${local.app_name}-voting-lambda-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
}

resource "aws_iam_policy" "voting_lambda_policy" {
  name = "${local.app_name}-voting-lambda-policy"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "dynamodb:GetItem"
        ]
        Resource = [
          aws_dynamodb_table.voting_profiles.arn,
          aws_dynamodb_table.session_metadata.arn
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject"
        ]
        Resource = "${aws_s3_bucket.polly_universe.arn}/*"
      },
      {
        Effect = "Allow"
        Action = [
          "ssm:GetParameter",
          "ssm:GetParameters"
        ]
        Resource = [
          aws_ssm_parameter.bot_token.arn,
          aws_ssm_parameter.notifications_peer_id.arn,
          aws_ssm_parameter.openai_api_key.arn
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "kms:Decrypt"
        ]
        Resource = "*"
        Condition = {
          StringEquals = {
            "kms:ViaService" = "ssm.${data.aws_region.current.name}.amazonaws.com"
          }
        }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "voting_lambda_basic_execution" {
  role       = aws_iam_role.voting_lambda_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_role_policy_attachment" "voting_lambda_policy_attachment" {
  role       = aws_iam_role.voting_lambda_role.name
  policy_arn = aws_iam_policy.voting_lambda_policy.arn
}

resource "aws_cloudwatch_log_group" "voting_lambda_logs" {
  name              = "/aws/lambda/${local.app_name}-voting"
  retention_in_days = 14
}
