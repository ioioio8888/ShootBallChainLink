// SPDX-License-Identifier: MIT
pragma solidity ^0.8.4;

import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";
import "@chainlink/contracts/src/v0.8/KeeperCompatible.sol";
import "@chainlink/contracts/src/v0.8/ChainlinkClient.sol";
import "@chainlink/contracts/src/v0.8/ConfirmedOwner.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";

contract Game is
    KeeperCompatibleInterface,
    ReentrancyGuard,
    AccessControl,
    ChainlinkClient,
    ConfirmedOwner
{
    enum GameState {
        NotStart,
        Starting,
        Finished
    }

    event playerReadyEvent(address _playerAddress, uint256 _teamNumber);
    event RewardDistributedEvent(address[] winnerAddresses, uint256 reward);
    event Withdraw(uint256 balance);

    using Chainlink for Chainlink.Request;

    uint256 public volume;
    bytes32 private jobId;
    uint256 private fee;

    event RequestVolume(bytes32 indexed requestId, uint256 volume);

    GameState public currentState;

    mapping(address => uint256) public addressToTeam;
    mapping(uint256 => address[]) public teamToAddresses;

    uint256 public teamSize;
    uint256 public totalBalance;
    uint256 public entranceFee;
    uint256 public taxFee = 100000000000000; // 0.0001eth
    uint256 public winningTeam;

    bytes32 public constant ADMIN_ROLE = keccak256("GAME_ADMIN");

    constructor(uint256 _teamSize, uint256 _entranceFee)
        ConfirmedOwner(msg.sender)
    {
        require(_teamSize > 0, "WRONG_TEAM_SIZE");
        require(_entranceFee > 0, "WRONG_ENTRANCE_FEE");
        totalBalance = 0;
        winningTeam = 0;
        teamSize = _teamSize;
        entranceFee = _entranceFee;
        currentState = GameState.NotStart;

        setChainlinkToken(0xa36085F69e2889c224210F603D836748e7dC0088);
        setChainlinkOracle(0x74EcC8Bdeb76F2C6760eD2dc8A46ca5e581fA656);
        jobId = "ca98366cc7314957b8c012c72f05aeeb";
        fee = (1 * LINK_DIVISIBILITY) / 10;
        _setupRole(ADMIN_ROLE, msg.sender);
    }

    function requestVolumeData() public returns (bytes32 requestId) {
        require(hasRole(ADMIN_ROLE, msg.sender), "CALLER_IS_NOT_ADMIN");

        Chainlink.Request memory req = buildChainlinkRequest(
            jobId,
            address(this),
            this.fulfill.selector
        );

        req.add(
            "get",
            "https://l3fynv0esl.execute-api.ap-southeast-1.amazonaws.com/dev/game"
        );

        req.add("path", "body,winner");

        int256 timesAmount = 1;
        req.addInt("times", timesAmount);

        // Sends the request
        return sendChainlinkRequest(req, fee);
    }

    /**
     * Receive the response in the form of uint256
     */
    function fulfill(bytes32 _requestId, uint256 _volume)
        public
        recordChainlinkFulfillment(_requestId)
    {
        emit RequestVolume(_requestId, _volume);
        winningTeam = _volume;
        currentState = GameState.Finished;
    }

    /**
     * Allow withdraw of Link tokens from the contract
     */
    function withdrawLink() public onlyOwner {
        LinkTokenInterface link = LinkTokenInterface(chainlinkTokenAddress());
        require(
            link.transfer(msg.sender, link.balanceOf(address(this))),
            "Unable to transfer"
        );
    }

    function setAdminRole(address _gameAdmin) external onlyOwner {
        _setupRole(ADMIN_ROLE, _gameAdmin);
    }

    function setTeamSize(uint256 _teamSize) external onlyOwner {
        require(_teamSize > 0, "WRONG_TEAM_SIZE");
        teamSize = _teamSize;
    }

    function setTaxFee(uint256 _taxFee) external onlyOwner {
        require(entranceFee >= 0, "WRONG_TAX_FEE");
        taxFee = _taxFee;
    }

    function setEntranceFee(uint256 _entranceFee) external onlyOwner {
        require(entranceFee > 0, "WRONG_ENTRANCE_FEE");
        entranceFee = _entranceFee;
    }

    function setWinningTeam(uint256 _teamNumber) external onlyOwner {
        require(_teamNumber > 0 && _teamNumber < 4, "INVALID_TEAM_NUMBER");
        winningTeam = _teamNumber;
        currentState = GameState.Finished;
    }

    function playerReady(uint256 _teamNumber) public payable nonReentrant {
        require(_teamNumber > 0 && _teamNumber < 3, "INVALID_TEAM_NUMBER");
        require(entranceFee == msg.value, "NOT_ENOUGH_FOR_ENTRY");
        require(
            teamToAddresses[_teamNumber].length <= teamSize,
            "TEAM_IS_FULL"
        );
        require(
            _checkAlreadyEntered(msg.sender, _teamNumber) == false,
            "ALREADY_ENTER"
        );

        addressToTeam[msg.sender] = _teamNumber;
        teamToAddresses[_teamNumber].push(msg.sender);
        totalBalance += msg.value;

        if (
            teamToAddresses[1].length == teamSize &&
            teamToAddresses[2].length == teamSize
        ) {
            currentState = GameState.Starting;
        }

        emit playerReadyEvent(msg.sender, _teamNumber);
    }

    function distributeReward() internal {
        uint256 reward = (totalBalance - taxFee) / teamSize;
        address[] memory winnerAddresses = teamToAddresses[winningTeam];
        for (uint256 i = 0; i < teamSize; i++) {
            (bool sent, bytes memory data) = payable(winnerAddresses[i]).call{
                value: reward
            }("");
            require(sent, "Failed to send Reward");
        }
        restartGame();
        emit RewardDistributedEvent(winnerAddresses, reward);
    }

    function _checkAlreadyEntered(address _playerAddress, uint256 _teamNumber)
        internal
        view
        returns (bool)
    {
        address[] memory teamAddresses = teamToAddresses[_teamNumber];
        for (uint256 i = 0; i < teamAddresses.length; i++) {
            if (teamAddresses[i] == _playerAddress) {
                return true;
            }
        }
        return false;
    }

    function restartGame() internal {
        for (uint256 i = 0; i < teamToAddresses[1].length; i++) {
            delete addressToTeam[teamToAddresses[1][i]];
        }
        for (uint256 i = 0; i < teamToAddresses[2].length; i++) {
            delete addressToTeam[teamToAddresses[2][i]];
        }
        delete teamToAddresses[1];
        delete teamToAddresses[2];

        totalBalance = 0;
        winningTeam = 0;
        currentState = GameState.NotStart;
    }

    function checkUpkeep(
        bytes calldata /* checkData */
    )
        external
        view
        override
        returns (
            bool upkeepNeeded,
            bytes memory /* performData */
        )
    {
        upkeepNeeded =
            currentState == GameState.Finished &&
            winningTeam != 0 &&
            winningTeam != 3;
    }

    function performUpkeep(
        bytes calldata /* performData */
    ) external override {
        if (
            currentState == GameState.Finished &&
            winningTeam != 0 &&
            winningTeam != 3
        ) {
            distributeReward();
        }
    }

    function withdraw() external onlyOwner {
        uint256 balance = address(this).balance;
        require(balance > 0, "Withdraw: NO_NATIVE_TOKEN_BALANCE");
        payable(msg.sender).transfer(balance);
        emit Withdraw(balance);
    }
}
