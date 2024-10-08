import {  util } from '@aws-appsync/utils';
export function request(
  ctx
) {
  const item = ctx.args.input;
  const id = util.autoId()

  return {
    operation: 'PutItem',
    key: util.dynamodb.toMapValues({
      PK: "USER",
      SK: `USER#${id}`
    }),
    attributeValues: util.dynamodb.toMapValues({
      id: id,
      ENTITY: "USER",
      ...item,
    }),
  };
}

export function response(
  ctx
) {
  return ctx.result;
}