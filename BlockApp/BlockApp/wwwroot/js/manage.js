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