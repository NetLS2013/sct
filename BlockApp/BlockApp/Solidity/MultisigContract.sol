pragma solidity ^0.4.0;
contract MultiSig {

    mapping (address => uint) public balances;
    mapping (address => bool) public confirmations;
    mapping (address => bool) public isOwner;
    address[] public owners;
    uint[] public withdraws;
    uint public required;
    bool isReadyToSafe;
    bool public isExecuted;
    uint public requiredDeposit;

    modifier ownerExists(address owner) {
        require(isOwner[owner]);
        _;
    }

    modifier confirmed(address owner) {
        require(confirmations[owner]);
        _;
    }

    modifier notConfirmed(address owner) {
        require(!confirmations[owner]);
        _;
    }

    modifier notExecuted() {
        require(!isExecuted);
        _;
    }

    modifier ready() {
        require(isReadyToSafe);
        _;
    }

    modifier notReady() {
        require(!isReadyToSafe);
        _;
    }

    event Execution();
    event ExecutionFailure();
    event Deposit(address indexed sender, uint256 value);
    event Withdraw(address indexed sender, uint256 value);
    event WithdrawFailture(address indexed sender);
    event Confirmation(address indexed sender);

    function MultiSig(address[] _owners, uint[] _withdraws, uint _require, uint _requiredDeposit) public {
        for (uint j = 0; j < _owners.length; j++) {
            isOwner[_owners[j]] = true;
        }

        withdraws = _withdraws;
        owners = _owners;
        required = _require;

        requiredDeposit = _requiredDeposit;

        isExecuted = false;
        isReadyToSafe = false;
    }

    function deposit() payable public ownerExists(msg.sender) notReady() {
        balances[msg.sender] += msg.value;

        isReadyToSafe = isReady();
        Deposit(msg.sender, msg.value);
    }

    function withdraw() public ownerExists(msg.sender) notReady() {
        if (balances[msg.sender] != 0) {
            uint amount = balances[msg.sender];
            msg.sender.transfer(amount);
            
            balances[msg.sender] = 0;
            Withdraw(msg.sender, amount);
        }

        WithdrawFailture(msg.sender);
    }

    function confirmTransaction() public ownerExists(msg.sender) notConfirmed(msg.sender) ready() {
        confirmations[msg.sender] = true;
        Confirmation(msg.sender);
    }

    function executeTransaction() public ownerExists(msg.sender) confirmed(msg.sender) notExecuted() ready() {
        if (isConfirmed()) {
            for (uint i = 0; i < owners.length; i++) {
                owners[i].transfer(withdraws[i]);
                balances[owners[i]] = 0;
            }
            isExecuted = true;
            Execution();
        }

        ExecutionFailure();
    }

    function isConfirmed() public constant returns (bool) {
        uint count = 0;
        for (uint i = 0; i < owners.length; i++) {
            if (confirmations[owners[i]])
                count += 1;
            if (count == required)
                return true;
        }
    }

    function isReady() public constant returns (bool) {
        if(!isReadyToSafe) {
            for (uint i = 0; i < owners.length; i++) {
                if (requiredDeposit != balances[owners[i]]) {
                    return false;
                }
            }
        }

        return true;
    }
}
