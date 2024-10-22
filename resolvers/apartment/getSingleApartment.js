import {get} from "@aws-appsync/utils/dynamodb"

export function request(ctx){
    return get({key:{PK:`BUILDING#${ctx.args.buildingId}`, SK:`APARTMENT#${ctx.args.apartmentId}`}})
}

export function response(ctx){
    return ctx.result;
}