const extractHashFromUrl = (url: string): { url: string; hash: string | null; } => {
    const hashIndex = url.lastIndexOf("#");
    if (hashIndex > 0)
        return {
            url: url.substring(0, hashIndex),
            hash: url.substring(hashIndex + 1)
        };
    return { url, hash: null };
}

export {
    extractHashFromUrl
}