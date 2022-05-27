//lambda set winner function

const {Pool} = require('pg')
const ethers = require("ethers");
const contract = require("./game.json");

const contract_address = process.env.CONTRACT;
const provider = new ethers.providers.JsonRpcProvider(
    "https://speedy-nodes-nyc.moralis.io/807fb02272b21813c742e8c5/eth/kovan"
);
const privateKey = process.env.PRIVATEKEY;
const wallet = new ethers.Wallet(privateKey, provider);

// init contract instance
const game_contract = new ethers.Contract(
    contract_address,
    contract.abi,
    wallet
);

const pool = new Pool({
    host     : process.env.RDS_HOSTNAME,
    user     : process.env.RDS_USERNAME,
    password : process.env.RDS_PASSWORD,
    port     : process.env.RDS_PORT,
    database : process.env.RDS_DB_NAME
});

exports.handler = async (event)  => {

    const sql = "UPDATE game SET winner = ($1) WHERE guid = ($2) and winner = 0 RETURNING guid,winner;";
    const { winner , guid } = event;
    let values = [winner,guid];
    // let values = [1,'test_game_0']

    let responseBody;

    const client = await pool.connect();
    await client
        .query(sql, values)
        .then(res => {
            responseBody = res.rows[0];
            console.log("DB Return : ",res.rows);
        })
        .catch(e => {
            responseBody = e.stack;
        });

    const apiRequest = await game_contract.requestVolumeData();
    console.log("Request API data :", apiRequest);

    const response = {
        "statusCode": 200,
        "body":  responseBody,
        "isBase64Encoded": false
    };
    return response;

};

