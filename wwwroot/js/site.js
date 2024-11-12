// Initialize vote count and state from the data attribute, with fallback to 0
const initialScore = parseInt(document.getElementById('vote-count').dataset.initialScore, 10) || 0;
let voteCount = initialScore; // Set voteCount to the initial score only once
let hasUpvoted = false;
let hasDownvoted = false;

// Function to update the vote count display
function updateVoteCount() {
    document.getElementById('vote-count').textContent = voteCount;
}

// Initialize the display with the initial vote count
updateVoteCount();

// Initialize Lottie animations
const upvoteAnimation = lottie.loadAnimation({
    container: document.getElementById('upvote-animation'),
    renderer: 'svg',
    loop: false,
    autoplay: false,
    path: '/Images/arrow-up.json'
});

const downvoteAnimation = lottie.loadAnimation({
    container: document.getElementById('downvote-animation'),
    renderer: 'svg',
    loop: false,
    autoplay: false,
    path: '/Images/arrow-down.json'
});

// Add event listeners for upvote button (only play animation, do not modify voteCount here)
const upvoteButton = document.querySelector('.upvote');
upvoteButton.addEventListener('click', () => {
    if (!hasUpvoted) {
        hasUpvoted = true;
        hasDownvoted = false;
    } else {
        hasUpvoted = false;
    }
    upvoteAnimation.play();
});

upvoteButton.addEventListener('mouseenter', () => upvoteAnimation.play());
upvoteButton.addEventListener('mouseleave', () => upvoteAnimation.stop());

// Add event listeners for downvote button (only play animation, do not modify voteCount here)
const downvoteButton = document.querySelector('.downvote');
downvoteButton.addEventListener('click', () => {
    if (!hasDownvoted) {
        hasDownvoted = true;
        hasUpvoted = false;
    } else {
        hasDownvoted = false;
    }
    downvoteAnimation.play();
});

downvoteButton.addEventListener('mouseenter', () => downvoteAnimation.play());
downvoteButton.addEventListener('mouseleave', () => downvoteAnimation.stop());
