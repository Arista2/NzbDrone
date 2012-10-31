﻿//URLs
var addSeriesUrl = '/addseries/addexistingseries';
var addNewSeriesUrl = '/addseries/addnewseries';
var existingSeriesUrl = '/addseries/existingseries';
var addNewUrl = '/addseries/addnew';

var deleteRootDirUrl = '/addseries/deleterootdir';
var saveRootDirUrl = '/addseries/saverootdir';
var rootListUrl = '/addseries/rootlist';


//ExistingSeries
$(".masterQualitySelector").live('change', function () {

    var profileId = $(this).val();
    $("#existingSeries").find(".qualitySelector").each(function () {
        $(this).val(profileId);
    });
});

$(".addExistingButton").live('click', function () {
    var button = $(this);
    $(button).attr('disabled', 'disabled');
    var root = $(this).parents(".existingSeries");
    var title = $(this).siblings(".seriesLookup").val();
    var seriesId = $(this).siblings(".seriesId").val();
    var qualityId = $(this).siblings(".qualitySelector").val();
    var date = $(this).siblings('.start-date').val();

    var path = root.find(".seriesPathValue Label").text();

    if (seriesId === 0 || $.trim(title).length === 0) {
        $.gritter.add({
            title: 'Failed',
            text: 'Invalid Series Information for \'' + path + '\'',
            icon: 'icon-minus-sign',
            class_name: 'gritter-fail'
        });

        return false;
    }

    $.ajax({
        type: "POST",
        url: addSeriesUrl,
        data: jQuery.param({ path: path, seriesName: title, seriesId: seriesId, qualityProfileId: qualityId, startDate: date }),
        error: function (req, status, error) {
            $(button).removeAttr('disabled');
        },
        success: function() {
            root.hide('highlight', 'fast');
            if ($('.existingSeries').filter(":visible").length === 1)
                reloadExistingSeries();
        }
    });

});

function reloadExistingSeries() {
    $('#existingSeries').html('<img src="../../Content/Images/ajax-loader.gif" />');
    $.ajax({
      url: existingSeriesUrl,
      success: function( data ) {
        $('#existingSeries').html(data);
      }
    });
}

$(".start-date-master").live('change', function () {

    var date = $(this).val();
    $("#existingSeries").find(".start-date").each(function () {
        $(this).val(date);
    });
});

//RootDir
//Delete RootDir
$(document).on('click', '.delete-root', function () {
    var path = $(this).attr('data-path');

    $.ajax({
        type: "POST",
        url: deleteRootDirUrl,
        data: { Path: path },
        success: function () {
            refreshRoot();
            $("#rootDirInput").val('');
        }
    });
});

$('#saveDir').live('click', saveRootDir);

function saveRootDir() {
    var path = $("#rootDirInput").val();

    if (path) {
        $.ajax({
            type: "POST",
            url: saveRootDirUrl,
            data: { Path: path },
            success: function () {
                refreshRoot();
                $("#rootDirInput").val('');
            }
        });
    }
}

function refreshRoot() {
    $.ajax({
        url: rootListUrl,
        success: function (data) {
            $('#rootDirs').html(data);
            reloadAddNew();
            reloadExistingSeries();
        }
    });
}


//AddNew
$('#saveNewSeries').live('click', function () {
    $('#saveNewSeries').attr('disabled', 'disabled');

    var seriesTitle = $("#newSeriesLookup").val();
    var seriesId = $("#newSeriesId").val();
    var qualityId = $("#qualityList").val();
    var path = $('#newSeriesPath').val();
    var date = $('#newStartDate').val();

    $.ajax({
        type: "POST",
        url: addNewSeriesUrl,
        data: jQuery.param({ path: path, seriesName: seriesTitle, seriesId: seriesId, qualityProfileId: qualityId, startDate: date }),
        error: function (req, status, error) {
            $('#saveNewSeries').removeAttr('disabled');
        },
        success: function () {
            $('#saveNewSeries').removeAttr('disabled');
            $("#newSeriesLookup").val('');
            $('#newStartDate').val('');
        }
    });
});

function reloadAddNew() {
    $.ajax({
        url: addNewUrl,
        success: function (data) {
            $('#addNewSeries').html(data);
        }
    });
}


//Watermark
$('#rootDirInput').livequery(function () {
    $(this).watermark('Enter your new root folder path...');
});

$('#newSeriesLookup').livequery(function () {
    $(this).watermark('Title of the series you want to add...');
});

$('.existingSeriesContainer .seriesLookup').livequery(function () {
    $(this).watermark('Please enter the series title...');
});