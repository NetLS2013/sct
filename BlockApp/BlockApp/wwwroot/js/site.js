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
