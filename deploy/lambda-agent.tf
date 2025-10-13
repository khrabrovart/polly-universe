data "archive_file" "agent_lambda_zip" {
  type        = "zip"
  source_dir  = "../agent-lambda"
  output_path = "agent-lambda.zip"
}

resource "aws_lambda_function" "agent_lambda" {
  filename      = data.archive_file.agent_lambda_zip.output_path
  function_name = "${local.app_name}-agent"
  role          = aws_iam_role.agent_lambda_role.arn
  handler       = "PollyUniverse.Func.Agent::PollyUniverse.Func.Agent.Function::Handle"
  runtime       = "dotnet8"
  timeout       = 30
  memory_size   = 512
  architectures = ["arm64"]

  source_code_hash = data.archive_file.agent_lambda_zip.output_base64sha256

  environment {
    variables = {
      S3_BUCKET                = aws_s3_bucket.polly_universe.bucket
      SESSION_METADATA_TABLE   = aws_dynamodb_table.session_metadata.name
      VOTING_PROFILES_TABLE    = aws_dynamodb_table.voting_profiles.name
      BOT_TOKEN_PARAMETER      = aws_ssm_parameter.bot_token.name
      OPENAI_API_KEY_PARAMETER = aws_ssm_parameter.openai_api_key.name
      OPENAI_MODEL             = var.openai_model
      HISTORY_LENGTH           = var.agent_history_length
    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.agent_lambda_basic_execution,
    aws_cloudwatch_log_group.agent_lambda_logs
  ]
}

resource "aws_iam_role" "agent_lambda_role" {
  name = "${local.app_name}-agent-lambda-role"

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

resource "aws_iam_policy" "agent_lambda_policy" {
  name = "${local.app_name}-agent-lambda-policy"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:UpdateItem",
          "dynamodb:Query",
          "dynamodb:Scan",
          "dynamodb:BatchGetItem"
        ]
        Resource = [
          aws_dynamodb_table.voting_profiles.arn,
          aws_dynamodb_table.session_metadata.arn
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "s3:ListBucket"
        ]
        Resource = aws_s3_bucket.polly_universe.arn
      },
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject",
          "s3:PutObject",
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
            "kms:ViaService" = "ssm.${data.aws_region.current.id}.amazonaws.com"
          }
        }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "agent_lambda_basic_execution" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
  role       = aws_iam_role.agent_lambda_role.name
}

resource "aws_iam_role_policy_attachment" "agent_lambda_policy_attachment" {
  role       = aws_iam_role.agent_lambda_role.name
  policy_arn = aws_iam_policy.agent_lambda_policy.arn
}

resource "aws_cloudwatch_log_group" "agent_lambda_logs" {
  name              = "/aws/lambda/${local.app_name}-agent"
  retention_in_days = 14
}
