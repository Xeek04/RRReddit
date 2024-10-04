// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Initialize vote count and state
let voteCount = 0;
let hasUpvoted = false; // Track if the user has upvoted
let hasDownvoted = false; // Track if the user has downvoted

// Initialize Lottie animations
const upvoteAnimation = lottie.loadAnimation({
    container: document.getElementById('upvote-animation'), // the DOM element for upvote
    renderer: 'svg',
    loop: false,
    autoplay: false, // Animation does not play automatically
    path: '/Images/arrow-up.json' // Path to your Lottie JSON for upvote
});

const downvoteAnimation = lottie.loadAnimation({
    container: document.getElementById('downvote-animation'), // the DOM element for downvote
    renderer: 'svg',
    loop: false,
    autoplay: false, // Animation does not play automatically
    path: '/Images/arrow-down.json' // Path to your Lottie JSON for downvote
});

// Add event listeners for upvote button
const upvoteButton = document.querySelector('.upvote');
upvoteButton.addEventListener('click', () => {
    if (hasDownvoted) {
        voteCount++; // Switch from downvote to upvote
        hasDownvoted = false; // Reset downvote state
    } else if (!hasUpvoted) {
        voteCount++; // Increase vote count if not already upvoted
    }
    hasUpvoted = true; // Mark as upvoted
    updateVoteCount();
    upvoteAnimation.play(); // Play animation on click
});
upvoteButton.addEventListener('mouseenter', () => {
    upvoteAnimation.play();
});
upvoteButton.addEventListener('mouseleave', () => {
    upvoteAnimation.stop();
});

// Add event listeners for downvote button
const downvoteButton = document.querySelector('.downvote');
downvoteButton.addEventListener('click', () => {
    if (hasUpvoted) {
        voteCount--; // Switch from upvote to downvote
        hasUpvoted = false; // Reset upvote state
    } else if (!hasDownvoted) {
        voteCount--; // Decrease vote count if not already downvoted
    }
    hasDownvoted = true; // Mark as downvoted
    updateVoteCount();
    downvoteAnimation.play(); // Play animation on click
});
downvoteButton.addEventListener('mouseenter', () => {
    downvoteAnimation.play();
});
downvoteButton.addEventListener('mouseleave', () => {
    downvoteAnimation.stop();
});

// Function to update the vote count display
function updateVoteCount() {
    document.getElementById('vote-count').textContent = voteCount;
}
