(function($) {
    $("#btnGenerateUnconfirmedScript").on("click", function(e) {
        var button = $(this);

        if (!$(this.form).valid()) {
            return;
        }

        e.preventDefault();
        
        $.ajax({
            type: "POST",
            url: "/Purchaser/GenerateUnconfirmedScript",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({
                hash: $("#hash").val(),
                xPubKey: $("#XPubKey").val(),
                email: $("#Email").val()
            }),
            success: function(response) {
                $("#UnconfirmedDepositTx").val(response.unconfirmedScript);
                toast("Data seved.", "alert-success");
            },
            error: function(response) {
                toast(response.responseText, "alert-danger");
            },
            beforeSend: function() {
                button.parent().find(".loader").css("display", "inline-block");
            },
            complete: function() {
                button.parent().find(".loader").hide();
            }
        });
    });

    $("#btnSendPartiallyDepositScript").on("click", function() {
        var button = $(this);
        
        $.ajax({
            type: "POST",
            url: "/Purchaser/SendPartiallyDepositScript",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({
                hash: $("#hash").val(),
                partiallySignedScript: $("#BuyerConfirmedDepositTx").val()
            }),
            success: function(response) {
                console.log(response);
            },
            error: function(response) {
                console.log(response);
            },
            beforeSend: function() {
                button.parent().find(".loader").css("display", "inline-block");
            },
            complete: function() {
                button.parent().find(".loader").hide();
            }
        });
    });

    $("#btnSendPartiallyWithdrawScript").on("click", function() {
        var button = $(this);
        
        $.ajax({
            type: "POST",
            url: "/Purchaser/SendPartiallyWithdrawScript",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({
                hash: $("#hash").val(),
                partiallySignedScript: $("#BuyerConfirmedWithdrawTx").val()
            }),
            success: function(response) {
                console.log(response);
            },
            error: function(response) {
                console.log(response);
            },
            beforeSend: function() {
                button.parent().find(".loader").css("display", "inline-block");
            },
            complete: function() {
                button.parent().find(".loader").hide();
            }
        });
    });

    $("#btnSaveDetail").on("click", function (e) {
        var button = $(this);

        if (!$(this.form).valid()) {
            return;
        }

        e.preventDefault();

        $.ajax({
            type: "POST",
            url: "/Purchaser/SaveDataEthereum",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({
                hash: $("#hash").val(),
                email: $("#Email").val(),
                Address: $("#BuyerAddress").val()
            }),
            success: function (response) {
                $("#FeeAddress").val(response.address);
                $("#FeeValue").val(response.fee);
                $("#Balance").val("0");

                $("#btnSaveDetail").prop("disabled", true);
                
                toast("Data seved.", "alert-success");
            },
            error: function (response) {
                toast(response.responseText, "alert-danger");
            },
            beforeSend: function () {
                button.parent().find(".loader").css("display", "inline-block");
            },
            complete: function () {
                button.parent().find(".loader").hide();
            }
        });
    });
    $("#btnCheckBalance").on("click", function () {
        var button = $(this);

        $.ajax({
            type: "POST",
            url: "/Purchaser/Balance",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({
                hash: $("#hash").val()
            }),
            success: function (response) {
                $("#Balance").val(response.balance);
                
                if (response.balance > 0) {
                    $("#btnDeploy").prop("disabled", false);
                }
            },
            error: function (response) {
                console.log(response);
            },
            beforeSend: function () {
                button.parents(".form-btn-loader").find(".loader").css("display", "inline-block");
            },
            complete: function () {
                button.parents(".form-btn-loader").find(".loader").hide();
            }
        });
    });
    $("#btnDeploy").on("click", function () {
        var button = $(this);

        $.ajax({
            type: "POST",
            url: "/Purchaser/DeployContract",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({
                hash: $("#hash").val()
            }),
            success: function (response) {
                $("#ContractAddress").val(response.contractAddress);
                $("#ABI").val(response.abi);

                $("#btnDeploy").prop("disabled", true);
            },
            error: function (response) {
                console.log(response);
            },
            beforeSend: function () {
                button.parent().find(".loader").css("display", "inline-block");
            },
            complete: function () {
                button.parent().find(".loader").hide();
            }
        });
    });
}(jQuery));