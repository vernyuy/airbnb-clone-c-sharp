import {  util } from '@aws-appsync/utils';
import { put } from '@aws-appsync/utils/dynamodb';
export function request(
  ctx
) {
  const input = ctx.args.input;
  const id = util.autoId()
  const item = {
    id: id,
    ENTITY: "USER",
    ...input,
    createdAt: util.time.nowISO8601(),
    updatedAt: util.time.nowISO8601(),
  }
  const key = {
    PK: "USER",
    SK: `USER#${id}`,
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