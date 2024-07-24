const extractHashFromUrl = (url: string): { url: string; hash: string | null; } => {
    const hashIndex = url.indexOf("#");
    if (hashIndex > 0) {
        const hash = url.substring(hashIndex + 1);
        if (hash)
            return { url: url.substring(0, hashIndex), hash };
    }

    return { url, hash: null };
}

export {
    extractHashFromUrl
}