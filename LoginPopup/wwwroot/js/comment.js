// Function to add the comment icon and form under each post dynamically
function addCommentIcons() {
    const posts = document.querySelectorAll('.card');
    posts.forEach(post => {
        // Create and append the comment icon
        const commentIcon = document.createElement('div');
        commentIcon.classList.add('comment-icon');
        commentIcon.innerHTML = '<i class="bi bi-chat-square" style="font-size: 24px;"></i>';
        post.appendChild(commentIcon);

        // Create the comment form (initially hidden) and append to each post
        const commentForm = document.createElement('div');
        commentForm.classList.add('comment-form');
        commentForm.innerHTML = `
            <textarea class="form-control comment-input" rows="3" placeholder="Pour your thoughts..."></textarea>
            <button class="btn btn-primary mt-2 comment-btn">Comment</button>
        `;
        commentForm.style.display = 'none';
        post.appendChild(commentForm);

        // Create and append the comment section to each post
        const commentSection = document.createElement('div');
        commentSection.classList.add('comment-section');
        post.appendChild(commentSection);

        // Add event listener for toggling the form
        commentIcon.addEventListener('click', () => {
            commentForm.style.display = commentForm.style.display === 'none' || commentForm.style.display === '' ? 'block' : 'none';
        });

        // Add event listener for posting a comment
        const commentBtn = commentForm.querySelector('.comment-btn');
        commentBtn.addEventListener('click', () => postComment(post));
    });
}

// Function to post the comment to the correct post
function postComment(post) {
    const commentInput = post.querySelector('.comment-input');
    const commentText = commentInput.value.trim();

    if (commentText !== '') {
        const commentSection = post.querySelector('.comment-section');
        const newComment = document.createElement('p');
        newComment.textContent = commentText;
        commentSection.appendChild(newComment);

        // Clear the input after posting
        commentInput.value = '';
    } else {
        alert('Please write a comment before posting!');
    }
}

// Add comment icons to posts on page load
window.onload = addCommentIcons;
