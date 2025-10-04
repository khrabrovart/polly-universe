data "archive_file" "voting_lambda_zip" {
  type        = "zip"
  source_dir  = "../voting-lambda"
  output_path = "voting-lambda.zip"
}

resource "aws_lambda_function" "voting_lambda" {
  filename      = data.archive_file.voting_lambda_zip.output_path
  function_name = "${local.app_name}-voting"
  role          = aws_iam_role.voting_lambda_role.arn
  handler       = "bootstrap"
  runtime       = "provided.al2"
  timeout       = 300
  memory_size   = 256
  architectures = ["arm64"]

  source_code_hash = data.archive_file.voting_lambda_zip.output_base64sha256

  environment {
    variables = {
      Voting__TelegramClientDataTable = aws_dynamodb_table.telegram_client_data.name
      Voting__VotingProfilesTable     = aws_dynamodb_table.voting_profiles.name
      Voting__S3Bucket                = aws_s3_bucket.polly_universe.bucket
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
          aws_dynamodb_table.telegram_client_data.arn
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject"
        ]
        Resource = "${aws_s3_bucket.polly_universe.arn}/*"
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
