// require("@nomiclabs/hardhat-waffle");
// require("@nomiclabs/hardhat-truffle5");
// require("@nomiclabs/hardhat-etherscan");
// require("@nomiclabs/hardhat-solhint");
// require("@nomiclabs/hardhat-web3");
require("dotenv").config();
// require("solidity-coverage");

const config = {
  defaultNetwork: "hardhat",
  networks: {
    hardhat: {
      accounts: { count: 100 },
    },
    rinkeby: {
      url: `https://speedy-nodes-nyc.moralis.io/${process.env.MORALIS_API_KEY}/eth/rinkeby`,
      accounts: [`${process.env.PRIVATE_KEY}`],
    },
    mainnet: {
      url: `https://mainnet.infura.io/v3/${process.env.INFURA_API_KEY}`,
      accounts: [`${process.env.PRIVATE_KEY}`],
    },
  },
  accounts: {
    count: 100,
  },
  etherscan: {
    apiKey: `${process.env.ETHERSCAN_KEY}`,
  },
  solidity: {
    compilers: [
      {
        version: "0.8.4",
        settings: {
          optimizer: {
            enabled: true,
            runs: 200,
          },
        },
      },
    ],
  },
  paths: {
    sources: "./contracts",
    tests: "./test",
    cache: "./cache",
    artifacts: "./artifacts",
    deploy: "deploy",
    deployments: "deployments",
  },
};

module.exports = config;
