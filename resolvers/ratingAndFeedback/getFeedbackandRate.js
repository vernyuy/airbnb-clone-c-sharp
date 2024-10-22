import {utils} from "@aws-appsync/utils"

export function request(ctx) {
    const buildingId = ctx.args.buildingId
  return {
    operation: "Query",
    query: {
        expression: "PK = :pk and begins_with(SK, :sk)",
        expressionValues: utils.dynamodb.toMapValues({
            ":pk": `APARTMENT#${buildingId}`,
            ":sk": "FEEDBACK#"
        })
    }
  }
}

export function response(ctx) {
  return ctx.result;
}
