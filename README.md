# ShootBall - A Web3 game, make use of Chainlink Keeper
## A Fun,Crazy Multiplayer soccer Game, with multi-characters with crazy skills!

![image](https://user-images.githubusercontent.com/19797490/170442151-fa584387-8c20-4823-b230-3646cee44586.png)
![2022-05-26 18-50-46_Moment](https://user-images.githubusercontent.com/19797490/170704220-08d44392-5e72-4683-bbd1-4e9e4aab5ea5.jpg)
![2022-05-26 18-50-46_Trim (1)](https://user-images.githubusercontent.com/19797490/170704832-38e1b8b1-2bbf-48f4-8490-83d70d1b8871.gif)
![2022-05-26 18-50-46_Moment2](https://user-images.githubusercontent.com/19797490/170704249-52b585f0-aae2-423e-895b-05e17fb39132.jpg)

Shoot ball is a online multiplayer soccer game which allows 2-6 player to compete and bet by using their coins as enterance fee.
A portion will be taken from the smart contract as service fee. The winner of the game will share the pool of prize(all enterance fee - service fee).

## Game Controls
WASD - movement control
Left Shift - Running
Space - Jumping
1 - Cast Skill 1
2 - Cast Skill 2
Left Click - Charge To Low Kick
Left Click - Charge To High kick

## Get Started
At least two people to test the game. It is a multiplayer game.
Prerequisite: a browser with a wallet supports web3(with some Kovan testnet eth ~0.01eth) and connect to Ethereum Kovan Testnet(Recommend to use metaMask)

1. Go to the link https://ioioio8888.github.io/ShootBallChainLink/
2. Press the start button in starting screen.
3. Connect your wallet which is in Kovan network.
4. Wait for the game to load.
5. Type in a Display name if you are first time login.
6. Click on custom game button
7. (room master only)Type in a uniqe room name and click on the Create Game Button if you want to create a game as game master
8. (join game only)Type in a room name that the room master has created and click on the join room button, or click on join game in the list of room
9. Click on the ready game button.
10. Interact with the smart contract in your wallet to pay the enterance fee.
11. Pick your favourite character
12. (room master only)Start the game when everyone is ready.
13. if someone haven't paid the entarnce fee, a error message will show up.
14. Enjoy the game.
15. When the game ends, the prize will distributed to the corresponding players.

## Tech Stack
### Blockchain - Smart Contract
  The contract is built with Chainlink Keeper and API Request Job on Kovan.
  The contract will distribute the reward automatically to each team members equally based on the game result got from the API.
  After distributed all the reward to winning team, the contract would clean the records and restart for the next game.
  

### Game
  Unity Engine - For game development
  Moralis - for web3 api, authentication and smart contract interaction
  Photon Network - for Multiplayer networking
  Playfab - for authentication and backend logic
  

