﻿//URLs
var addSeriesUrl = '../AddSeries/AddExistingSeries';
var addNewSeriesUrl = '../AddSeries/AddNewSeries';
var existingSeriesUrl = '../AddSeries/ExistingSeries';
var addNewUrl = '../AddSeries/AddNew';

var deleteRootDirUrl = '../AddSeries/DeleteRootDir';
var saveRootDirUrl = '../AddSeries/SaveRootDir';
var rootListUrl = '../AddSeries/RootList';


//ExistingSeries
$(".masterQualitySelector").live('change', function () {

    var profileId = $(this).val();
    $("#existingSeries").find(".qualitySelector").each(function () {
        $(this).val(profileId);
    });
});

$(".addExistingButton").live('click', function () {

    var root = $(this).parents(".existingSeries");

    var title = $(this).siblings(".seriesLookup").val();
    var qualityId = $(this).siblings(".qualitySelector").val();

    var path = root.find(".seriesPathValue Label").text();

    $.ajax({
        type: "POST",
        url: addSeriesUrl,
        data: jQuery.param({ path: path, seriesName: title, qualityProfileId: qualityId }),
        error: function (req, status, error) {
            alert("Sorry! We could not add " + path + " at this time. " + error);
        },
        success: function () {
            root.hide('highlight', 'fast');
        }
    });

});

function reloadExistingSeries() {
    $.get(existingSeriesUrl, function (data) {
        $('#existingSeries').html(data);
    });
}

//RootDir
//$('#rootDirInput').watermark('Start typing to add new root folder...');

$('#rootDirs .actionButton img').live('click', function (image) {
    var path = $(image.target).attr('id');
    $.post(deleteRootDirUrl, { Path: path }, function () {
        refreshRoot();
    });
});

$('#saveDir').live('click', saveRootDir);

function saveRootDir() {
    var path = $("#rootDirInput").val();
    if (path) {
        $.post(saveRootDirUrl, { Path: path }, function () {
            refreshRoot();
            $("#rootDirInput").val('');
        });
    }
}

function refreshRoot() {
    $.get(rootListUrl, function (data) {
        $('#rootDirs').html(data);
    });
    reloadAddNew();
    reloadExistingSeries();    
}


//AddNew
//$('#newSeriesPath').watermark('Path for the new series...');
//$('#newSeriesLookup').watermark('Title of the series you want to add...');

$('#saveNewSeries').live('click', function () {
    var seriesTitle = $("#newSeriesLookup").val();
    var qualityId = $("#qualityList").val();
    var path = $('#newSeriesPath').val();

    $.ajax({
        type: "POST",
        url: addNewSeriesUrl,
        data: jQuery.param({ path: path, seriesName: seriesTitle, qualityProfileId: qualityId }),
        error: function (req, status, error) {
            alert("Sorry! We could not add " + path + " at this time. " + error);
        },
        success: function () {
            $("#newSeriesLookup").val("");
            //$('#newSeriesPath').val("");
        }
    });
});

function reloadAddNew() {
    $.get(addNewUrl, function (data) {
        $('#addNewSeries').html(data);
    });
}


//On load
jQuery(document).ready(function () {
    //RootDir
    $('#rootDirInput').watermark('Start typing to add new root folder...');

    //AddNew
    $('#newSeriesPath').watermark('Path for the new series...');
    $('#newSeriesLookup').watermark('Title of the series you want to add...');
});