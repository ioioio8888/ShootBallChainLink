//lambda set winner function

const {Pool} = require('pg')
const ethers = require("ethers");
const contract = require("./game.json");

const provider = new ethers.providers.JsonRpcProvider(
    "https://speedy-nodes-nyc.moralis.io/807fb02272b21813c742e8c5/eth/kovan"
);
const privateKey =
    "8d528872351ea7e6120797eedac07f56d55ee5e77c3f0f212735bea09123410e";
const wallet = new ethers.Wallet(privateKey, provider);
const contract_address = "0x9B8139a4EE3C294a15c76Ec982AE8e8073a6108F";

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

    const sql = "UPDATE game SET winner = ($1) WHERE guid = ($2) and winner IS NULL RETURNING guid,winner;";
    const { winner , guid } = event;
    let values = [winner,guid];
    // let values = [1,'test_game_0']

    let responseBody;

    const client = await pool.connect();
    await client
        .query(sql, values)
        .then(res => {
            responseBody = res.rows[0];
            console.log("DB Return : ",res.rows[0]);
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

