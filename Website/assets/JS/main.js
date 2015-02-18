var body;
var hashtagSelectionSidebar;
var hashtagSelectionSidebarOuterWidth;
var openHashtagSelectionSidebarButton;
var closeHashtagSelectionSidebarButton;

// Ensure no JavaScript is performed until the D.O.M. has loaded.
$(function () {
	// Initialise global variables.
	body = $('body');
	hashtagSelectionSidebar = $('#hashtagSelectionSidebar');
	hashtagSelectionSidebarOuterWidth = hashtagSelectionSidebar.outerWidth();
	openHashtagSelectionSidebarButton = $('#openHashtagSelectionSidebarButton');
	closeHashtagSelectionSidebarButton = $('#closeHashtagSelectionSidebarButton');

	// Resize the hashtag selection sidebar.
	resizeHashtagSelectionSidebar();

	// Initialise event listeners.
	$(window).resize(resizeHashtagSelectionSidebar);

	openHashtagSelectionSidebarButton.click(toggleHashtagSelectionSidebarVisibility);
	closeHashtagSelectionSidebarButton.click(toggleHashtagSelectionSidebarVisibility);

	$('#hashtags li').click(function () {
		alert('Hashtag \"' + $(this).text() + '\" was selected.');
	});

	$('#modalLinks li').click(function () {
		var modalElement = $('#' + $(this).text() + 'ModalBox');

		displayElementModally(modalElement);
	});

	$('#optionsButton').click(function () {
		var modalElement = $('#optionsModalBox');

		displayElementModally(modalElement);
	});
});

// Resize the hashtag selection sidebar.
function resizeHashtagSelectionSidebar() {
	var windowHeight = $(window).height();

	hashtagSelectionSidebar.outerHeight(windowHeight);
}

// Toggle the hashtag selection sidebar's visibility.
var hashtagSelectionSidebarVisible = true;

function toggleHashtagSelectionSidebarVisibility() {
	// Hide sidebar.
	if(hashtagSelectionSidebarVisible) {
		hashtagSelectionSidebar.animate({'left': -hashtagSelectionSidebarOuterWidth + 'px'}, 1000, function () {
			openHashtagSelectionSidebarButton.fadeIn();

			hashtagSelectionSidebarVisible = false;
		});

		return;
	}

	// Display sidebar.
	openHashtagSelectionSidebarButton.fadeOut(function () {
		hashtagSelectionSidebar.animate({'left': '0px'}, 1000);

		hashtagSelectionSidebarVisible = true;
	});
}

// Display an element modally.
var modalFreezeFrame;
var modalElementPresent = false;
var modalElement;

function displayElementModally(element) {
	// Ensure the modal freeze frame is available.
	if(!modalFreezeFrame) {
		body.append('<div id="modalFreezeFrame"></div>').css('overflow', 'hidden');

		modalFreezeFrame = $('#modalFreezeFrame');
	}

	// Present element modally.
	if(!modalElementPresent) {
		modalFreezeFrame.fadeIn(function () {
			centreElement(element);
			element.css({'z-index': '1', 'box-shadow': '0px 0px 5px #007C9E'}).fadeIn(function () {
				modalElementPresent = true;
				modalElement = element;
			});
		});

		// Allow modal element to be dismissed by pressing the "esc" key.
		$(document).keyup(function (keyEvent) {
			if(keyEvent.keyCode == 27) {
				removeModalElement();
			}
		});

		// Allow modal element to be dismissed by clicking the modal freeze frame.
		modalFreezeFrame.click(removeModalElement);
	}
}

// Remove the current modal element.
function removeModalElement() {
	if(modalElementPresent) {
		modalElement.fadeOut(function () {
			modalFreezeFrame.fadeOut(function () {
				modalElementPresent = false;
				modalElement = null;
			});
		});
	}
}

// Centre an element absolutely.
function centreElement(element) {
	var newTopPosition = (($(window).height() - element.outerHeight()) / 2) + 'px';
	var newLeftPosition = (($(window).width() - element.outerWidth()) / 2) + 'px';

	element.css({'position': 'absolute', 'top': newTopPosition, 'left': newLeftPosition});
}