import {utils} from "@aws-appsync/utils"

export function request(ctx) {
    const buildId = ctx.args.buildingId
  return {
    operation: "Query",
    query: {
        expression: "PK = :pk and begins_with(SK, :sk)",
        expressionValues: utils.dynamodb.toMapValues({
            ":pk": `BUILDING#${buildId}`,
            ":sk": "APARTMENT#"
        })
    }
  }
}

export function response(ctx) {
  return ctx.result;
}
