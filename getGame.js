//lambda get game function

const {Pool} = require('pg')

const pool = new Pool({
    host     : process.env.RDS_HOSTNAME,
    user     : process.env.RDS_USERNAME,
    password : process.env.RDS_PASSWORD,
    port     : process.env.RDS_PORT,
    database: process.env.RDS_DB_NAME,
});

exports.handler = async (event, context, callback)  => {

    const sql = "SELECT guid,winner FROM game WHERE winner IS NOT NULL;";
    let responseBody;

    const client = await pool.connect();
    await client
        .query(sql)
        .then(res => {
            responseBody = res.rows[0];
            console.log("DB Return : ",res.rows[0]);
        })
        .catch(e => {
            responseBody = e.stack;
        });

    const response = {
        "statusCode": 200,
        // "headers": {
        //     "header": "staking_tier_header",
        //     "Access-Control-Allow-Origin" : "*",
        //     "Access-Control-Allow-Credentials" : true
        // },
        "body":  responseBody,
        "isBase64Encoded": false
    };
    return response;

};
