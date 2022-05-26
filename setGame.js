//lambda set game function
const {Pool} = require('pg')

const pool = new Pool({
    host     : process.env.RDS_HOSTNAME,
    user     : process.env.RDS_USERNAME,
    password : process.env.RDS_PASSWORD,
    port     : process.env.RDS_PORT,
    database : process.env.RDS_DB_NAME,
});

exports.handler = async (event)  => {
    // const {player_red,player_blue,guid} = JSON.stringify(event.body);

    const sql = "INSERT INTO game(player_red, player_blue, guid) VALUES ($1 , $2 , $3) RETURNING *;";
    const {player_red,player_blue,guid} = event;
    let values = [player_red, player_blue, guid];

    let responseBody;
    const client = await pool.connect();
    await client
        .query(sql, values)
        .then(res => {
            responseBody = res.rows[0];
            console.log("DB Return : ",res.rows[0]);
        })
        .catch(e => {
            responseBody = event.body;
            responseBody = e.stack;
        });

    const response = {
        "statusCode": 200,
        // "headers": {
        //     // "header": "staking_tier_header",
        //     "Content-Type" : 'application/json',
        //     "Access-Control-Allow-Origin" : "*",
        //     "Access-Control-Allow-Methods" : "*",
        //     "Access-Control-Allow-Headers" : "*",
        //     "Access-Control-Allow-Credentials" : true
        // },
        // "body":  JSON.stringify(responseBody),
        "body":  responseBody,
        "isBase64Encoded": false
    };
    return response;

};
