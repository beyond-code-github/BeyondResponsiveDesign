function touchScrollFix() {
    // Disable scroll for the document, we'll handle it ourselves
    $(document).on('touchmove', function (e) {
        e.preventDefault();
    });

    // Check if we should allow scrolling up or down
    $(document.body).on("touchstart", ".vscroll", function (e) {
        // If the element is scrollable (content overflows), then...
        if (this.scrollHeight !== this.clientHeight) {
            // If we're at the top, scroll down one pixel to allow scrolling up
            if (this.scrollTop === 0) {
                this.scrollTop = 1;
            }
            // If we're at the bottom, scroll up one pixel to allow scrolling down
            if (this.scrollTop === this.scrollHeight - this.clientHeight) {
                this.scrollTop = this.scrollHeight - this.clientHeight - 1;
            }
        }
        // Check if we can scroll
        this.allowUp = this.scrollTop > 0;
        this.allowDown = this.scrollTop < (this.scrollHeight - this.clientHeight);
        this.lastY = e.originalEvent.pageY;
    });

    $(document.body).on('touchmove', ".vscroll", function (e) {
        var event = e.originalEvent;
        var up = event.pageY > this.lastY;
        var down = !up;
        this.lastY = event.pageY;

        if ((up && this.allowUp) || (down && this.allowDown)) {
            event.stopPropagation();
        } else {
            event.preventDefault();
        }
    });
}