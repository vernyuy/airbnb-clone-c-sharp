import {  util } from '@aws-appsync/utils';
export function request(
  ctx
) {
  const item = ctx.args.input;
  const id = util.autoId()

  return {
    operation: 'PutItem',
    attributeValues: util.dynamodb.toMapValues({
      id: id,
      ENTITY: "USER",
      ...item,
      PK: "USER",
      SK: `USER#${id}`,
      createdAt: util.time.nowISO8601(),
      updatedAt: util.time.nowISO8601(),
    }),
  };
}

export function response(
  ctx
) {
  return ctx.result;
}