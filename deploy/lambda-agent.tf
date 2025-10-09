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

    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.agent_lambda_basic_execution,
    aws_iam_role_policy_attachment.agent_lambda_policy_attachment,
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

resource "aws_iam_role_policy_attachment" "agent_lambda_basic_execution" {
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
  role       = aws_iam_role.agent_lambda_role.name
}

resource "aws_iam_policy" "agent_lambda_policy" {
  name        = "${local.app_name}-agent-lambda-policy"
  description = "IAM policy for agent lambda function"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
    ]
  })
}

resource "aws_iam_role_policy_attachment" "agent_lambda_policy_attachment" {
  policy_arn = aws_iam_policy.agent_lambda_policy.arn
  role       = aws_iam_role.agent_lambda_role.name
}

resource "aws_cloudwatch_log_group" "agent_lambda_logs" {
  name              = "/aws/lambda/${local.app_name}-agent"
  retention_in_days = 14
}
