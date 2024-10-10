import {get} from "@aws-appsync/utils/dynamodb"

export function request(ctx){
    const {id} = ctx.args
    return get({key:{PK:"USER", SK:`USER#${id}`}})
}

export function response(ctx){
    return ctx.result;
}