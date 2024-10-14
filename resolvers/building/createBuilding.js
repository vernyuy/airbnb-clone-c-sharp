import {  util } from '@aws-appsync/utils';
import { put } from '@aws-appsync/utils/dynamodb';
export function request(
  ctx
) {
  const input = ctx.args.input;
  const id = util.autoId()
  const item = {
    id: id,
    GSI2PK: `USER#${item.userId}`,
    GSI2SK: `BUILDING#${id}`,
    ENTITY: "BUILDING",
    ...input,
    createdAt: util.time.nowISO8601(),
    updatedAt: util.time.nowISO8601(),
  }
  const key = {
    PK: "BUILDING",
    SK: `BUILDING#${id}`,
  }

  return put({
    key: key,
    item: item
  })
}

export function response(
  ctx
) {
  return ctx.result;
}