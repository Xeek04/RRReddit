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
    if (hasUpvoted) {
        // If already upvoted, clicking again will remove the upvote
        voteCount--;
        hasUpvoted = false;
    } else {
        if (hasDownvoted) {
            // If previously downvoted, clicking upvote will cancel downvote and bring to neutral
            voteCount++;
            hasDownvoted = false;
        }
        // Clicking upvote will increase the vote count by 1
        voteCount++;
        hasUpvoted = true;
    }
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
    if (hasDownvoted) {
        // If already downvoted, clicking again will remove the downvote
        voteCount++;
        hasDownvoted = false;
    } else {
        if (hasUpvoted) {
            // If previously upvoted, clicking downvote will cancel upvote and bring to neutral
            voteCount--;
            hasUpvoted = false;
        }
        // Clicking downvote will decrease the vote count by 1
        voteCount--;
        hasDownvoted = true;
    }
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