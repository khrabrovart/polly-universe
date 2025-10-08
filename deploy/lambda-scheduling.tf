data "archive_file" "scheduling_lambda_zip" {
  type        = "zip"
  source_dir  = "../scheduling-lambda"
  output_path = "scheduling-lambda.zip"
}

resource "aws_lambda_function" "scheduling_lambda" {
  filename      = data.archive_file.scheduling_lambda_zip.output_path
  function_name = "${local.app_name}-scheduling"
  role          = aws_iam_role.scheduling_lambda_role.arn
  handler       = "PollyUniverse.Func.Scheduling::PollyUniverse.Func.Scheduling.Function::Handle"
  runtime       = "dotnet8"
  timeout       = 60
  memory_size   = 512
  architectures = ["arm64"]

  source_code_hash = data.archive_file.scheduling_lambda_zip.output_base64sha256

  environment {
    variables = {

    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.scheduling_lambda_basic_execution,
    aws_iam_role_policy_attachment.scheduling_lambda_policy_attachment,
    aws_cloudwatch_log_group.scheduling_lambda_logs
  ]
}

resource "aws_iam_role" "scheduling_lambda_role" {
  name = "${local.app_name}-scheduling-lambda-role"

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

resource "aws_iam_policy" "scheduling_lambda_policy" {
  name = "${local.app_name}-scheduling-lambda-policy"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "dynamodb:DescribeStream",
          "dynamodb:GetRecords",
          "dynamodb:GetShardIterator",
          "dynamodb:ListStreams"
        ]
        Resource = "${aws_dynamodb_table.voting_profiles.arn}/stream/*"
      },
      {
        Effect = "Allow"
        Action = [
          "scheduler:CreateSchedule",
          "scheduler:UpdateSchedule",
          "scheduler:DeleteSchedule",
          "scheduler:GetSchedule",
          "scheduler:ListSchedules"
        ]
        Resource = [
          "arn:aws:scheduler:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:schedule/${aws_scheduler_schedule_group.scheduler_group.name}/*"
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "iam:PassRole"
        ]
        Resource = aws_iam_role.scheduler_role.arn
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "scheduling_lambda_basic_execution" {
  role       = aws_iam_role.scheduling_lambda_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_role_policy_attachment" "scheduling_lambda_policy_attachment" {
  role       = aws_iam_role.scheduling_lambda_role.name
  policy_arn = aws_iam_policy.scheduling_lambda_policy.arn
}

resource "aws_cloudwatch_log_group" "scheduling_lambda_logs" {
  name              = "/aws/lambda/${local.app_name}-scheduling"
  retention_in_days = 14
}

resource "aws_lambda_event_source_mapping" "voting_profiles_stream" {
  event_source_arn                   = aws_dynamodb_table.voting_profiles.stream_arn
  function_name                      = aws_lambda_function.scheduling_lambda.arn
  starting_position                  = "LATEST"
  batch_size                         = 10
  maximum_batching_window_in_seconds = 5

  depends_on = [
    aws_iam_role_policy_attachment.scheduling_lambda_basic_execution,
    aws_iam_role_policy_attachment.scheduling_lambda_policy_attachment
  ]
}
