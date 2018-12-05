function download(filename, text) {
    var element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
    element.setAttribute('download', filename);

    element.style.display = 'none';
    document.body.appendChild(element);

    element.click();

    document.body.removeChild(element);

    toast("Downloaded!", "alert-success");
}

function copy(elem) {
    elem.select();
    document.execCommand("Copy");

    toast("Copied!", "alert-success");
}

function toast(text, color) {
    if (!color) {
        color = "alert-info";
    }

    $("#alertBox").addClass("show");
    $("#alertText").text(text);
    $("#alertColor").addClass(color);

    setTimeout(function () {
        $("#alertBox").removeClass("show");
        $("#alertColor").removeClass();
    }, 3000);
}

$(function () {
    var submitActor = null;
    var $form = $('#tableForm');
    var $submitActors = $form.find('button');
    $submitActors.click(function (event) {
        submitActor = this;
    });
    $('#tableForm').submit(function () {
        var page = Number($("input[name=page]").val());
        if (submitActor.className.indexOf("button-left") !== -1) {
            page--;
        }
        if (submitActor.className.indexOf("button-right") !== -1) {
            page++;
        }
        if (submitActor.className.indexOf("per-page") !== -1) {
            $("input[name=itemPerPage]").val(Number(submitActor.innerText));
        }
        if (submitActor.className.indexOf("button-sort") !== -1) {
            var rightSort = "date";
            var currSort = $("input[name=sortOrder]").val();
            if (submitActor.className.indexOf("amount-sort") !== -1) {
                if (currSort != "amount" || currSort == "amount_desc") {
                    rightSort = "amount";
                } else if (currSort == "amount") {
                    rightSort = "amount_desc";
                }
            }
            if (submitActor.className.indexOf("date-sort") !== -1) {
                if (currSort == "date" || currSort != "date_desc") {
                    rightSort = "date_desc";
                } else if (currSort == "date_desc") {
                    rightSort = "date";
                }
            }
            $("input[name=sortOrder]").val(rightSort);
        }

        $("input[name=page]").val(page);
        return true;
    });



    $("#alertClose").click(function () {
        $("#alertBox").removeClass("show");
    });

    $(".btn-copy").click(function () {
        copy($(this.parentElement).prevAll("input").first());
    });

    $(".btn-download").click(function () {
        download("payment.txn", $(this.parentElement).prevAll("input").first().val());
    });

    $("input[name=trxBtnDeposit]").click(function () {

        var transationId = $("input[name='trxId']").val();
        var transationIdDeposit = $("input[name='trxIdDepo']").val();

        var reg = /^(?=.{64}$)[a-zA-Z0-9]*/;
        if (reg.test(transationIdDeposit)) {
            $.ajax({
                url: "/Manage/SendDepositTrxId",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                        TransactionId: transationId,
                        TransactionHash: transationIdDeposit
                    }),
                success: function (data) {
                    toast("Successfully saved!", "alert-success");
                },
                error: function (data) {
                    toast("Invalid transaction or user id!", "alert-danger");
                }
            });
        } else {
            toast("Wrong format of transaction id!", "alert-danger");
        }
        
    });

    $("input[name=trxBtnWithdraw]").click(function () {

        var transationId = $("input[name='trxId']").val();
        var transationIdwithdraw = $("input[name='trxIdWith']").val();

        var reg = /^(?=.{64}$)[a-zA-Z0-9]*/;
        if (reg.test(transationIdwithdraw)) {

            $.ajax({
                url: "/Manage/SendWithdrawTrxId",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                        transactionId: transationId,
                        transactionHash: transationIdwithdraw
                    }),
                success: function(data) {
                    toast("Successfully saved!", "alert-success");
                },
                error: function (data) {
                    toast("Invalid transaction or user id!", "alert-danger");
                }
            });
        } else {
            toast("Wrong format of transaction id!", "alert-danger");
        }
    });


    $(".menu-image").click(function() {
        $(".sidenav").addClass("showside");
        $("body").addClass("overfrow");
    });

    $(".button-close").click(function () {
        $(".sidenav").removeClass("showside");
        $("body").removeClass("overfrow");
    });

    $(".sidenav-body").click(function () {
        $(".sidenav").removeClass("showside");
        $("body").removeClass("overfrow");
    });

    $(window).resize(function () {
        if ($(window).width() >= 600) {
            $(".sidenav").removeClass("showside");
            $("body").removeClass("overfrow");
        }
    });

    $("#btnBack").click(function() {
        history.back();
    });

}(jQuery));

(function ($) {
    $("#btnGenerateMerchantSecret").on("click", function() {
        $.ajax({
            type: "POST",
            url: "/Manage/GenerateMerchantSecret",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function(response) {
                $("#MerchantSecret").val(response.merchantSecret);
            },
            error: function(response) {
                console.log(response);
            }
        });
    });

    $("#btnSendEmail").on("click", function () {
        var token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            type: "POST",
            url: "/Manage/SendVerificationEmail",
            data: {
                __RequestVerificationToken: token
            },
            success: function (response) {
                toast(response, "alert-success");
            },
            error: function (response) {
                toast(response.responseText, "alert-danger");
            }
        });
    });

    $(".dropdown-menu li a").on("click", function(e) {
        e.preventDefault();

        var text = $(this).text();
        var value = $(this).data('value');
        var button = $(this).parents(".dropdown").find('.btn');

        button.html(text + ' <span class="caret"></span>');
        button.val(value);

        $(".dropdown-menu li").removeClass();
        $(this).parent("li").addClass("active");

    });
    
    $("#dropdown-wallet li a").on("click", function(e) {
        e.preventDefault();
        
        $("#btnGenerateSubscribeAddress").prop("disabled", $("#wallet-type-value").val() == $(this).data('value'));
    });

    $("#btnGenerateSubscribeAddress").on("click", function() {
        var value = $("#choose-wallet").val();
        
        $.ajax({
            type: "POST",
            url: "/Manage/GenerateSubscribeAddress",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({
                walletType: value
            }),
            success: function(response) {
                $("#Address").val(response.address);
                $("#Price").val(response.price);

                $("#btnGenerateSubscribeAddress").prop("disabled", true);
                $("#wallet-type-value").val(value);
            },
            error: function(response) {
                console.log(response);
            }
        });
    });

    $("#btn-save-personal").on("click", function(e) {
        if (!$(this.form).valid()) {
            return;
        }
        
        e.preventDefault();            
        
        $.ajax({
            type: "POST",
            url: "/Manage/SavePersonalInfo",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                email: $("#Email").val(),
                firstName: $("#FirstName").val(),
                lastName: $("#LastName").val(),
                phoneNumber: $("#PhoneNumber").val()
            }),
            success: function(response) {
                toast(response, "alert-success");
            },
            error: function(response) {
                toast(response.responseText, "alert-danger");
            }
        });
    });

    $("#btn-save-password").on("click", function(e) {
        if (!$(this.form).valid()) {
            return;
        }

        e.preventDefault();

        $.ajax({
            type: "POST",
            url: "/Manage/ChangePassword",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                oldPassword: $("#OldPassword").val(),
                newPassword: $("#NewPassword").val(),
                confirmPassword: $("#ConfirmPassword").val()
            }),
            success: function(response) {
                toast(response, "alert-success");
            },
            error: function(response) {
                toast(response.responseText, "alert-danger");
            }
        });
    });

    $("#btn-save-merchant").on("click", function (e) {
        if (!$(this.form).valid()) {
            return;
        }

        e.preventDefault();

        $.ajax({
            type: "POST",
            url: "/Manage/SaveMerchant",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                merchantId: $("#MerchantId").val(),
                merchantSecret: $("#MerchantSecret").val(),
                redirectUri: $("#RedirectUri").val() || null,
                xPubKey: $("#XPubKey").val(),
                ethereumAddress: $("#EthereumAddress").val()
            }),
            success: function (response) {
                toast(response, "alert-success");
            },
            error: function (response) {
                toast(response.responseText, "alert-danger");
            }
        });
    });

    $("#btn-check-status").on("click", function(e) {
        $.ajax({
            type: "POST",
            url: "/Manage/Subscriptions",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: $("#Price").val(),
            success: function(response) {
                var subscibeStatusElement = $("#subscibe-status");
                
                if (response.status) {
                    subscibeStatusElement.removeClass("text-danger");
                    subscibeStatusElement.addClass("text-success");
                    subscibeStatusElement.text("Active");

                    toast(response.statusMessage, "alert-success");
                } else {
                    toast(response.statusMessage, "alert-danger");
                }                
            },
            error: function(response) {
                toast(response, "alert-danger");
            }
        });
    });
}(jQuery));
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