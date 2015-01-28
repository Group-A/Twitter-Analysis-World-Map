$(function () {
	var mapContainer = $('#mapContainer');
	var hashtagSelectionSidebar = $('#hashtagSelectionSidebar');

	// Resizing the map container and navigational sidebar.
	function resizeMapContainerAndHashtagSelectionSidebar() {
		var windowHeight = $(window).height();

		mapContainer.height(windowHeight);
		hashtagSelectionSidebar.outerHeight(windowHeight);
	}

	resizeMapContainerAndHashtagSelectionSidebar();

	$(window).resize(function () {
		resizeMapContainerAndHashtagSelectionSidebar();
	});

	// Position map in its starting position.
	var map = $('#mapContainer img');

	map.load(function () {
		var newMapContainerScrollTop = ((map.height() - mapContainer.height()) / 3);
		var newMapContainerScrollLeft = ((map.width() - mapContainer.width()) / 2);

		mapContainer.scrollTop(newMapContainerScrollTop).scrollLeft(newMapContainerScrollLeft);
	});

	// Enabling the map to be dragged.
	var clicking = false;
	var previousXMouseCoordinate;
	var previousYMouseCoordinate;

	mapContainer.mousedown(function (mouseEvent) {
		mouseEvent.preventDefault();

		previousXMouseCoordinate = mouseEvent.clientX;
		previousYMouseCoordinate = mouseEvent.clientY;
		clicking = true;
	});

	$(document).mouseup(function () {
		clicking = false;
	});

	mapContainer.mousemove(function (mouseEvent) {
		if(clicking) {
			mouseEvent.preventDefault();

			var xAxisDirection = ((previousXMouseCoordinate - mouseEvent.clientX) > 0 ? 1 : -1);
			var yAxisDirection = ((previousYMouseCoordinate - mouseEvent.clientY) > 0 ? 1 : -1);

			mapContainer.scrollLeft(mapContainer.scrollLeft() + (previousXMouseCoordinate - mouseEvent.clientX));
			mapContainer.scrollTop(mapContainer.scrollTop() + (previousYMouseCoordinate - mouseEvent.clientY));

			previousXMouseCoordinate = mouseEvent.clientX;
			previousYMouseCoordinate = mouseEvent.clientY;
		}
	});

	mapContainer.mouseleave(function () {
		clicking = false;
	});

	// Enabling the hashtag list to be clickable.
	$('#hashtags li').click(function () {
		alert('Hashtag \"' + $(this).text() + '\" was selected.');
	});

	// Handle the opening and closing of the hashtag selection sidebar.
	var hashtagSelectionSidebarVisible = true;
	var hashtagSelectionSidebarWidth = hashtagSelectionSidebar.outerWidth();
	var openHashtagSelectionSidebarButton = $('#openHashtagSelectionSidebarButton');

	function toggleHashtagSelectionSidebarVisibility() {
		if(hashtagSelectionSidebarVisible) {
			hashtagSelectionSidebar.animate({'left': -hashtagSelectionSidebarWidth + 'px'}, 1000, function () {
				openHashtagSelectionSidebarButton.fadeIn();

				hashtagSelectionSidebarVisible = false;
			});

			return;
		}

		openHashtagSelectionSidebarButton.fadeOut(function () {
			hashtagSelectionSidebar.animate({'left': '0px'}, 1000);

			hashtagSelectionSidebarVisible = true;
		});
	}

	$('#hashtagSelectionSidebar #closeHashtagSelectionSidebarButton').click(function () {
		toggleHashtagSelectionSidebarVisibility();
	});

	$('#openHashtagSelectionSidebarButton').click(function () {
		toggleHashtagSelectionSidebarVisibility();
	});
});