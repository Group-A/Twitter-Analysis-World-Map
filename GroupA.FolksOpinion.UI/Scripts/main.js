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

	// Handle colour scheme selections.
	populateOptionsModalBox();

	// Initialise event listeners.
	$(window).resize(resizeHashtagSelectionSidebar);

	openHashtagSelectionSidebarButton.click(toggleHashtagSelectionSidebarVisibility);
	closeHashtagSelectionSidebarButton.click(toggleHashtagSelectionSidebarVisibility);

	$('#hashtagSelectionSidebar #modalLinks li').click(function () {
		var modalElement = $('#' + $(this).text() + 'ModalBox');

		displayElementModally(modalElement);
	});

	// Make hashtag selection sidebar scrollable.
	hashtagSelectionSidebar.makeScrollable();
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
			element.css({'z-index': 1, 'box-shadow': '0px 0px 5px #007C9E'}).fadeIn(function () {
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

// Multi-browser support for custom scroll bar.
(function ($) {
	$.fn.makeScrollable = function () {
		var scrollableElement = this;

		// Add scroll bar elements.
		scrollableElement.prepend('<div class="scrollBarTrack"><div class="scrollBar"></div></div>');

		// Retrieve newly added scroll bar elements.
		var scrollBarTrack = scrollableElement.find('.scrollBarTrack');
		var scrollBar = scrollBarTrack.find('.scrollBar');

		// Configure scroll elements' CSS.
		var viewportHeight = this.outerHeight();
		var documentHeight = this[0].scrollHeight;
		var scrollBarTrackHeight = (viewportHeight - 10);
		var scrollBarHeight = ((viewportHeight / documentHeight) * viewportHeight);

		scrollBarTrack.height(scrollBarTrackHeight + 'px');
		scrollBar.height(scrollBarHeight + 'px').draggable({
				 	axis: 'y',
				 	containment: 'parent',
				 	drag: function () {
				 		var scrollBarTop = parseInt($(this).css('top'));
				 		var newScrollBarTop = ((scrollBarTop / scrollBarTrackHeight) * documentHeight);

				 		scrollBarTrack.css('top', (newScrollBarTop + 5) + 'px');
				 		scrollableElement.scrollTop(newScrollBarTop);
				 	}
		})

		return this;
	};
}(jQuery));