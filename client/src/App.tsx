import { useEffect, useMemo, useState } from 'react';
import type { FormEvent } from 'react';
import { API_BASE_URL, IDEAS_PER_PAGE_DEFAULT } from './config';

type Idea = {
  id: number;
  title: string;
  description: string | null;
  userId: number;
  createdAt: string;
  userVote: number | null;
};

type AuthState = {
  token: string;
  userId: number;
  username: string;
  email: string;
};

type AuthMode = 'signin' | 'register';

type ApiMessage = {
  message?: string;
};

const AUTH_STORAGE_KEY = 'ideas-to-vote-auth';

function loadAuthState(): AuthState | null {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as AuthState;
    if (!parsed.token || !parsed.userId) {
      return null;
    }

    return parsed;
  } catch {
    return null;
  }
}

function saveAuthState(auth: AuthState | null): void {
  if (!auth) {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    return;
  }

  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth));
}

async function parseMessage(response: Response): Promise<string> {
  try {
    const payload = (await response.json()) as ApiMessage;
    if (payload.message && payload.message.trim()) {
      return payload.message;
    }
  } catch {
    // Ignore non-json errors and use generic fallback.
  }

  return 'Request failed. Please try again.';
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString();
}

export function App() {
  const [auth, setAuth] = useState<AuthState | null>(() => loadAuthState());
  const [menuOpen, setMenuOpen] = useState(false);

  const [authMode, setAuthMode] = useState<AuthMode>('signin');
  const [authDialogOpen, setAuthDialogOpen] = useState(false);
  const [authLoading, setAuthLoading] = useState(false);
  const [authError, setAuthError] = useState<string>('');
  const [signInUsername, setSignInUsername] = useState('');
  const [signInPassword, setSignInPassword] = useState('');
  const [registerUsername, setRegisterUsername] = useState('');
  const [registerEmail, setRegisterEmail] = useState('');
  const [registerPassword, setRegisterPassword] = useState('');

  const [ideas, setIdeas] = useState<Idea[]>([]);
  const [ideasLoading, setIdeasLoading] = useState(false);
  const [ideasError, setIdeasError] = useState('');

  const [page, setPage] = useState(1);
  const pageSize = IDEAS_PER_PAGE_DEFAULT;

  const [addIdeaDialogOpen, setAddIdeaDialogOpen] = useState(false);
  const [addTitle, setAddTitle] = useState('');
  const [addDescription, setAddDescription] = useState('');
  const [addLoading, setAddLoading] = useState(false);
  const [addError, setAddError] = useState('');

  const [viewIdea, setViewIdea] = useState<Idea | null>(null);
  const [viewLoading, setViewLoading] = useState(false);
  const [viewError, setViewError] = useState('');
  const [editTitle, setEditTitle] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [editLoading, setEditLoading] = useState(false);

  const [ratings, setRatings] = useState<Record<number, number>>({});

  useEffect(() => {
    saveAuthState(auth);
  }, [auth]);

  useEffect(() => {
    if (!auth?.token) {
      setIdeas([]);
      setIdeasError('Sign in to load team ideas.');
      return;
    }

    void fetchIdeas(auth.token);
  }, [auth?.token]);

  useEffect(() => {
    const maxPage = Math.max(1, Math.ceil(ideas.length / pageSize));
    if (page > maxPage) {
      setPage(maxPage);
    }
  }, [ideas.length, page, pageSize]);

  const pageCount = Math.max(1, Math.ceil(ideas.length / pageSize));

  const pagedIdeas = useMemo(() => {
    const start = (page - 1) * pageSize;
    return ideas.slice(start, start + pageSize);
  }, [ideas, page, pageSize]);

  const isOwner = (idea: Idea): boolean => auth?.userId === idea.userId;

  async function fetchIdeas(token: string): Promise<void> {
    setIdeasLoading(true);
    setIdeasError('');

    try {
      const response = await fetch(`${API_BASE_URL}/api/ideas`, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });

      if (!response.ok) {
        setIdeasError(await parseMessage(response));
        setIdeas([]);
        return;
      }

      const payload = (await response.json()) as Idea[];
      setIdeas(payload);

      const initialRatings: Record<number, number> = {};
      for (const idea of payload) {
        if (idea.userVote !== null && idea.userVote !== undefined) {
          initialRatings[idea.id] = idea.userVote;
        }
      }
      setRatings(initialRatings);
    } catch {
      setIdeasError('Unable to connect to the API.');
      setIdeas([]);
    } finally {
      setIdeasLoading(false);
    }
  }

  function openAuth(mode: AuthMode): void {
    setAuthMode(mode);
    setAuthDialogOpen(true);
    setAuthError('');
    setMenuOpen(false);
  }

  function logout(): void {
    setAuth(null);
    setMenuOpen(false);
    setViewIdea(null);
    setAddIdeaDialogOpen(false);
  }

  async function submitIdea(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    if (!auth?.token) {
      setAddError('Sign in first.');
      return;
    }

    setAddLoading(true);
    setAddError('');

    try {
      const response = await fetch(`${API_BASE_URL}/api/ideas`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${auth.token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          title: addTitle,
          description: addDescription
        })
      });

      if (!response.ok) {
        setAddError(await parseMessage(response));
        return;
      }

      setAddTitle('');
      setAddDescription('');
      setAddIdeaDialogOpen(false);
      await fetchIdeas(auth.token);
      setPage(1);
    } catch {
      setAddError('Unable to connect to the API.');
    } finally {
      setAddLoading(false);
    }
  }

  async function submitSignIn(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setAuthLoading(true);
    setAuthError('');

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          username: signInUsername,
          password: signInPassword
        })
      });

      if (!response.ok) {
        setAuthError(await parseMessage(response));
        return;
      }

      const payload = (await response.json()) as AuthState;
      setAuth(payload);
      setAuthDialogOpen(false);
      setSignInPassword('');
    } catch {
      setAuthError('Unable to connect to the API.');
    } finally {
      setAuthLoading(false);
    }
  }

  async function submitRegister(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setAuthLoading(true);
    setAuthError('');

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          username: registerUsername,
          email: registerEmail,
          password: registerPassword
        })
      });

      if (!response.ok) {
        setAuthError(await parseMessage(response));
        return;
      }

      const payload = (await response.json()) as AuthState;
      setAuth(payload);
      setAuthDialogOpen(false);
      setRegisterPassword('');
    } catch {
      setAuthError('Unable to connect to the API.');
    } finally {
      setAuthLoading(false);
    }
  }

  async function openIdea(id: number): Promise<void> {
    if (!auth?.token) {
      return;
    }

    setViewLoading(true);
    setViewError('');

    try {
      const response = await fetch(`${API_BASE_URL}/api/ideas/${id}`, {
        headers: {
          Authorization: `Bearer ${auth.token}`
        }
      });

      if (!response.ok) {
        setViewError(await parseMessage(response));
        return;
      }

      const payload = (await response.json()) as Idea;
      setViewIdea(payload);
      setEditTitle(payload.title);
      setEditDescription(payload.description ?? '');
    } catch {
      setViewError('Unable to connect to the API.');
    } finally {
      setViewLoading(false);
    }
  }

  async function deleteIdea(id: number): Promise<void> {
    if (!auth?.token) {
      return;
    }

    const accepted = window.confirm('Delete this idea?');
    if (!accepted) {
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/api/ideas/${id}`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${auth.token}`
        }
      });

      if (!response.ok) {
        setIdeasError(await parseMessage(response));
        return;
      }

      await fetchIdeas(auth.token);
      if (viewIdea?.id === id) {
        setViewIdea(null);
      }
    } catch {
      setIdeasError('Unable to connect to the API.');
    }
  }

  async function updateIdea(event: FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();

    if (!auth?.token || !viewIdea) {
      return;
    }

    setEditLoading(true);
    setViewError('');

    try {
      const response = await fetch(`${API_BASE_URL}/api/ideas/${viewIdea.id}`, {
        method: 'PUT',
        headers: {
          Authorization: `Bearer ${auth.token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          title: editTitle,
          description: editDescription
        })
      });

      if (!response.ok) {
        setViewError(await parseMessage(response));
        return;
      }

      const payload = (await response.json()) as Idea;
      setViewIdea(payload);
      await fetchIdeas(auth.token);
    } catch {
      setViewError('Unable to connect to the API.');
    } finally {
      setEditLoading(false);
    }
  }

  async function rateIdea(id: number, value: number): Promise<void> {
    if (!auth?.token) return;

    setRatings((current) => ({ ...current, [id]: value }));

    try {
      await fetch(`${API_BASE_URL}/api/votes/${id}`, {
        method: 'PUT',
        headers: {
          Authorization: `Bearer ${auth.token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ value }),
      });
    } catch {
      // Optimistic update remains; silently ignore network errors
    }
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <div>
          <h1>Team ideas</h1>
          <p className="subtitle">View them! Like them!</p>
        </div>

        <div className="menu-wrap">
          {auth && <span className="greeting">Hello, {auth.username}!</span>}
          <button className="menu-toggle" type="button" onClick={() => setMenuOpen((v) => !v)} aria-label="Open menu">
            <img
              src={`https://api.dicebear.com/9.x/thumbs/svg?seed=${auth?.username ?? 'guest'}`}
              alt=""
              className="avatar"
            />
            <ChevronIcon open={menuOpen} />
          </button>
          {menuOpen && (
            <div className="dropdown" role="menu">
              {!auth && (
                <>
                  <button type="button" onClick={() => openAuth('signin')}>
                    Sign-in
                  </button>
                  <button type="button" onClick={() => openAuth('register')}>
                    Register
                  </button>
                </>
              )}
              {auth && (
                <button type="button" onClick={logout}>
                  Log out
                </button>
              )}
            </div>
          )}
        </div>
      </header>

      {!auth && (
        <section className="panel notice">
          <h2>Authentication required</h2>
          <p>Use the menu to sign in or register and load ideas.</p>
        </section>
      )}

      {auth && (
        <>
          <section className="panel ideas-section">
            <header className="ideas-section-head">
              <div>
                <h2>Team ideas</h2>
                <p className="subtitle-small">View them! Like them!</p>
              </div>
              <button
                type="button"
                className="icon-button add-idea-btn"
                aria-label="Add idea"
                onClick={() => {
                  setAddError('');
                  setAddIdeaDialogOpen(true);
                }}
              >
                <PlusIcon />
              </button>
            </header>

            {ideasLoading && <p className="state">Loading ideas...</p>}
            {!ideasLoading && ideasError && <p className="error">{ideasError}</p>}
            {!ideasLoading && !ideasError && pagedIdeas.length === 0 && <p className="state">No ideas yet.</p>}

            <div className="cards-grid">
              {pagedIdeas.map((idea) => (
                <article key={idea.id} className="idea-card">
                  <img src="/no-image.svg" alt="No image" className="idea-image" />
                  <div className="idea-body">
                    <div className="idea-main">
                      <div className="idea-copy">
                        <h3>{idea.title}</h3>
                        <p>{idea.description || 'No description provided.'}</p>
                        <small>Created {formatDate(idea.createdAt)}</small>
                      </div>

                      <div className="stars" aria-label="Rating stars">
                        {[1, 2, 3, 4, 5].map((value) => (
                          <button
                            key={value}
                            type="button"
                            className="star-button"
                            aria-label={`Rate ${value} stars`}
                            onClick={() => { void rateIdea(idea.id, value); }}
                          >
                            <StarIcon filled={(ratings[idea.id] ?? 0) >= value} />
                          </button>
                        ))}
                      </div>
                    </div>

                    <div className="idea-actions">
                      <button
                        type="button"
                        className="icon-button"
                        aria-label="View idea"
                        onClick={() => {
                          void openIdea(idea.id);
                        }}
                      >
                        <SearchIcon />
                      </button>

                      {isOwner(idea) && (
                        <button
                          type="button"
                          className="icon-button delete"
                          aria-label="Delete idea"
                          onClick={() => {
                            void deleteIdea(idea.id);
                          }}
                        >
                          <TrashIcon />
                        </button>
                      )}
                    </div>
                  </div>
                </article>
              ))}
            </div>

            <footer className="pagination">
              <button type="button" onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page <= 1}>
                Previous
              </button>
              <span>
                Page {page} / {pageCount}
              </span>
              <button type="button" onClick={() => setPage((p) => Math.min(pageCount, p + 1))} disabled={page >= pageCount}>
                Next
              </button>
            </footer>
          </section>
        </>
      )}

      {addIdeaDialogOpen && (
        <div className="modal-backdrop" onClick={() => setAddIdeaDialogOpen(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <header className="modal-head">
              <h2>Add idea</h2>
              <button type="button" onClick={() => setAddIdeaDialogOpen(false)} aria-label="Close">
                <CloseIcon />
              </button>
            </header>

            <form onSubmit={submitIdea} className="stack">
              <label>
                Title
                <input value={addTitle} onChange={(e) => setAddTitle(e.target.value)} maxLength={200} required />
              </label>
              <label>
                Description
                <textarea value={addDescription} onChange={(e) => setAddDescription(e.target.value)} maxLength={4000} rows={4} />
              </label>
              <button type="submit" disabled={addLoading}>
                {addLoading ? 'Adding...' : 'Add idea'}
              </button>
            </form>

            {addError && <p className="error">{addError}</p>}
          </div>
        </div>
      )}

      {authDialogOpen && (
        <div className="modal-backdrop" onClick={() => setAuthDialogOpen(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <header className="modal-head">
              <h2>{authMode === 'signin' ? 'Sign-in' : 'Register'}</h2>
              <button type="button" onClick={() => setAuthDialogOpen(false)} aria-label="Close">
                <CloseIcon />
              </button>
            </header>

            {authMode === 'signin' && (
              <form onSubmit={submitSignIn} className="stack">
                <label>
                  Username
                  <input value={signInUsername} onChange={(e) => setSignInUsername(e.target.value)} required />
                </label>
                <label>
                  Password
                  <input
                    type="password"
                    value={signInPassword}
                    onChange={(e) => setSignInPassword(e.target.value)}
                    required
                  />
                </label>
                <button type="submit" disabled={authLoading}>
                  {authLoading ? 'Signing in...' : 'Sign-in'}
                </button>
              </form>
            )}

            {authMode === 'register' && (
              <form onSubmit={submitRegister} className="stack">
                <label>
                  Username
                  <input value={registerUsername} onChange={(e) => setRegisterUsername(e.target.value)} required />
                </label>
                <label>
                  Email
                  <input type="email" value={registerEmail} onChange={(e) => setRegisterEmail(e.target.value)} required />
                </label>
                <label>
                  Password
                  <input
                    type="password"
                    value={registerPassword}
                    onChange={(e) => setRegisterPassword(e.target.value)}
                    required
                  />
                </label>
                <button type="submit" disabled={authLoading}>
                  {authLoading ? 'Creating account...' : 'Register'}
                </button>
              </form>
            )}

            {authError && <p className="error">{authError}</p>}
          </div>
        </div>
      )}

      {viewIdea && (
        <div className="modal-backdrop" onClick={() => setViewIdea(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <header className="modal-head">
              <h2>Idea details</h2>
              <button type="button" onClick={() => setViewIdea(null)} aria-label="Close">
                <CloseIcon />
              </button>
            </header>

            {viewLoading && <p className="state">Loading idea...</p>}
            {viewError && <p className="error">{viewError}</p>}

            {!viewLoading && (
              <div className="stack">
                <img src="/no-image.svg" alt="No image" className="idea-image detail" />
                <p>
                  <strong>Title:</strong> {viewIdea.title}
                </p>
                <p>
                  <strong>Description:</strong> {viewIdea.description || 'No description provided.'}
                </p>
                <p>
                  <strong>Created:</strong> {formatDate(viewIdea.createdAt)}
                </p>
              </div>
            )}

            {viewIdea && auth?.userId === viewIdea.userId && (
              <form onSubmit={updateIdea} className="stack editor">
                <h3>Edit your idea</h3>
                <label>
                  Title
                  <input value={editTitle} onChange={(e) => setEditTitle(e.target.value)} required maxLength={200} />
                </label>
                <label>
                  Description
                  <textarea
                    value={editDescription}
                    onChange={(e) => setEditDescription(e.target.value)}
                    rows={3}
                    maxLength={4000}
                  />
                </label>
                <button type="submit" disabled={editLoading}>
                  {editLoading ? 'Saving...' : 'Save changes'}
                </button>
              </form>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

function PlusIcon() {
  return (
    <svg viewBox="0 0 20 20" width="20" height="20" aria-hidden="true">
      <path d="M10 4.2v11.6M4.2 10h11.6" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  );
}

function SearchIcon() {
  return (
    <svg viewBox="0 0 20 20" width="20" height="20" aria-hidden="true">
      <circle cx="8.5" cy="8.5" r="5.5" fill="none" stroke="currentColor" strokeWidth="2.3" />
      <path d="M12.6 12.6l4.5 4.5" fill="none" stroke="currentColor" strokeWidth="2.3" strokeLinecap="round" />
    </svg>
  );
}

function TrashIcon() {
  return (
    <svg viewBox="0 0 20 20" width="20" height="20" aria-hidden="true">
      <path d="M4.5 5.3h11M7.2 5.3V3.9h5.6v1.4M8 8.2v6M12 8.2v6M6.6 5.3l.7 10.8h5.4l.7-10.8" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function StarIcon({ filled }: { filled: boolean }) {
  return (
    <svg viewBox="0 0 20 20" width="18" height="18" aria-hidden="true">
      <path
        d="M10 2.8l2.2 4.5 5 .7-3.6 3.5.8 5-4.4-2.3-4.4 2.3.8-5L2.8 8l5-.7L10 2.8z"
        fill={filled ? 'currentColor' : 'none'}
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function CloseIcon() {
  return (
    <svg viewBox="0 0 20 20" width="20" height="20" aria-hidden="true">
      <path d="M4.6 4.6l10.8 10.8M15.4 4.6L4.6 15.4" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  );
}

function ChevronIcon({ open }: { open: boolean }) {
  return (
    <svg
      viewBox="0 0 20 20"
      width="16"
      height="16"
      aria-hidden="true"
      style={{ transition: 'transform 0.2s', transform: open ? 'rotate(180deg)' : 'rotate(0deg)' }}
    >
      <path d="M4 7l6 6 6-6" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}
