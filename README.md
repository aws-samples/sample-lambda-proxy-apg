# Connecting .NET Lambda to Aurora PostgreSQL via RDS Proxy

Demo environment for the AWS Database Blog post: **Connecting .NET Lambda to Aurora PostgreSQL via RDS Proxy**

> **⚠️ IMPORTANT: This code is for demonstration purposes only and is NOT intended for production use.** This repository contains sample code to illustrate concepts discussed in the blog post. For production deployments, additional security hardening, error handling, monitoring, and architectural considerations are required.

## Overview

This repository contains infrastructure-as-code and sample application code demonstrating how to connect a .NET Lambda function to Aurora PostgreSQL using RDS Proxy for connection pooling and IAM authentication.

## Repository Contents

- `DBBLOG-4126-CFN.yaml` - CloudFormation template for provisioning the demo environment
- `Function.cs` - Sample C# Lambda function code demonstrating the connection pattern

## Architecture

The CloudFormation template provisions:

- **Aurora PostgreSQL Cluster** - Serverless v2 or provisioned database cluster
- **RDS Proxy** - Connection pooling and IAM authentication layer
- **EC2 Instance** - Bastion host for database access and testing
- **VPC Configuration** - Subnets, security groups, and networking
- **IAM Roles & Policies** - Permissions for Lambda and RDS Proxy

## Prerequisites

- AWS Account with appropriate permissions
- AWS CLI configured
- .NET SDK (for Lambda function development)

## Deployment

### 1. Deploy CloudFormation Stack

```bash
aws cloudformation create-stack \
  --stack-name aurora-rds-proxy-demo \
  --template-body file://DBBLOG-4126-CFN.yaml \
  --capabilities CAPABILITY_NAMED_IAM \
  --parameters ParameterKey=WinAdminPassword,ParameterValue='YourP@ssw0rd!'
```

### 2. Wait for Stack Completion

```bash
aws cloudformation wait stack-create-complete \
  --stack-name aurora-rds-proxy-demo
```

## Stack Outputs

After deployment, retrieve important connection details:

```bash
aws cloudformation describe-stacks \
  --stack-name aurora-rds-proxy-demo \
  --query 'Stacks[0].Outputs'
```

Key outputs include:
- RDS Proxy endpoint
- Aurora cluster endpoint
- EC2 instance ID
- Security group IDs

## Testing

1. Connect to the EC2 bastion host
2. Test database connectivity through RDS Proxy
3. Follow the instructions in the blog to deploy the Lambda function in the provisioned Windows EC2 instance.
4. Invoke the Lambda function to verify end-to-end connectivity to Aurora PostgreSQL through RDS Proxy.

## Cleanup

Delete the stack when finished:

```bash
aws cloudformation delete-stack \
  --stack-name aurora-rds-proxy-demo
```

## Security Considerations

- Database credentials are stored in AWS Secrets Manager
- IAM authentication is used for Lambda-to-RDS Proxy connections
- Security groups restrict access to necessary ports only
- All resources are deployed in private subnets (except bastion)

## Cost Optimization

This is a demo environment. To minimize costs:
- Use Aurora Serverless v2 with appropriate scaling configuration
- Delete the stack when not in use
- Consider using smaller EC2 instance types

## Related Resources

- [AWS Database Blog Post](https://aws.amazon.com/blogs/database/)
- [RDS Proxy Documentation](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/rds-proxy.html)
- [Aurora PostgreSQL Documentation](https://docs.aws.amazon.com/AmazonRDS/latest/AuroraUserGuide/Aurora.AuroraPostgreSQL.html)
- [AWS Lambda with .NET](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)

## License

This sample code is made available under the MIT-0 license. See the LICENSE file.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.
