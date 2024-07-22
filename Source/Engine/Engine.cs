using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Source.FrameDrawing;
using static Source.UtilityTypes.HMath;
using Source.UtilityTypes;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace Source.Engine
{
    public enum ColliderMode
    {
        Physical,
        Trigger,
        Static
    }
    public enum Side
    {
        None = 0,
        Left = -1,
        Right = 1,
        Top = -2,
        Bottom = 2,
    }

    public struct Size
    {
        public int Height;
        public int Width;
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public readonly Vector2 Center => new(Width / 2, Height / 2);
        public readonly Vector2 ToVector2() => new(Width, Height);
        public readonly Point ToPoint() => new(Width, Height);
    }
    public readonly struct Intersection
    {
        public readonly GameObject gameObject;
        public readonly Vector2 contactNormal;
        public readonly Vector2 depth;

        public Intersection(GameObject gameObject, Vector2 contactNormal, Vector2 depth)
        {
            this.gameObject = gameObject;
            this.contactNormal = contactNormal;
            this.depth = depth;
        }
    }

    public struct LineSegment
    {
        public Vector2 Start = Vector2.Zero;
        public Vector2 End = Vector2.Zero;
        public Point Center = Point.Zero;

        public readonly float Distance => Vector2.Distance(Start, End);
        public readonly Vector2 Direction => End - Start;
        public readonly Vector2 Normal => new(-Direction.X, -Direction.Y);

        public LineSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;

            Center = ((Start + End) / 2).ToPoint();
        }
        public readonly bool IsPointOn(Point point, float epsilon = 1e-6f)
        {
            float crossProduct = 
                (point.X -  Start.X) * (End.Y - Start.Y) - 
                (point.Y - Start.Y) * (End.X - Start.X);
            
            if (Math.Abs(crossProduct) > epsilon)
                return false;

            float dotProduct =
                (point.X - Start.X) * (End.X - Start.X) +
                (point.Y - Start.Y) * (End.Y - Start.Y);

            if (dotProduct < 0)
                return false;

            float squaredLengthBA =
                    (End.X - Start.X) * (End.X - Start.X) +
                    (End.Y - Start.Y) * (End.Y - Start.Y);

            if(dotProduct > squaredLengthBA) 
                return false;

            return true;
        }

        public readonly bool IntersectsWithRay(Vector2 rayOrigin, Vector2 rayDirection, out Vector2 intersectionPoint)
        {
            intersectionPoint = Vector2.Zero;

            float dx1 = Direction.X;
            float dy1 = Direction.Y;
            float dx2 = rayDirection.X;
            float dy2 = rayDirection.Y;

            float denominator = dy2 * dx1 - dx2 * dy1;

            if (denominator == 0)
                return false;

            float ua = (dx2 * (Start.Y - rayOrigin.Y) - dy2 * (Start.X - rayOrigin.X)) / denominator;
            float ub = (dx1 * (Start.Y - rayOrigin.Y) - dy1 * (Start.X - rayOrigin.X)) / denominator;

            if (ua >= 0 && ua <= 1 && ub >= 0)
            {
                intersectionPoint = new Vector2(Start.X + ua * dx1, Start.Y + ua * dy1);
                return true;
            }

            return false;
        }
        public readonly bool IntersectsWith(LineSegment other, out Vector2 intersectionPoint)
        {
            intersectionPoint = Vector2.Zero;

            float dx1 = Direction.X;
            float dy1 = Direction.Y;

            float dx2 = other.Direction.X;
            float dy2 = other.Direction.Y;

            float denominator = dy2 * dx1 - dx2 * dy1;

            if (denominator == 0)
                return false;

            float ua = (dx2 * (Start.Y - other.Start.Y) - dy2 * (Start.X - other.Start.X)) / denominator;
            float ub = (dx1 * (Start.Y - other.Start.Y) - dy1 * (Start.X - other.Start.X)) / denominator;

            if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
            {
                intersectionPoint = new(Start.X + ua * dx1, Start.Y + ua * dy1);
                return true;
            }

            return false;
        }
        public readonly bool IsCollinearWith(LineSegment other, float epsilon = 1e-6f)
        {
            Vector2 dir1 = Direction;
            Vector2 dir2 = other.Direction;

            float value = Math.Abs(dir1.X * dir2.Y - dir1.Y * dir2.X);
            bool result = value < epsilon;
            return result;
        }
        public readonly bool OverlapsWith(LineSegment other, float epsilon = 1e-6f)
        {
            if (!IsCollinearWith(other, epsilon))
                return false;

            Vector2 dir = Direction;
            
            float start1 = Vector2.Dot(Start, dir);
            float end1 = Vector2.Dot(End, dir);
            float start2 = Vector2.Dot(other.Start, dir);
            float end2 = Vector2.Dot(other.End, dir);

            if (start1 > end1)
                (start1, end1) = (end1, start1);

            if (start2 > end2)
                (start2, end2) = (end2, start2);

            return (start1 <= end2 + epsilon) && (start2 <= end1 + epsilon);
        }
        public readonly LineSegment? GetOverlappingSegment(LineSegment other)
        {
            if (IsCollinearWith(other) && OverlapsWith(other))
            {
                float minX = Math.Max(Math.Min(Start.X, End.X), Math.Min(other.Start.X, other.End.X));
                float maxX = Math.Min(Math.Max(Start.X, End.X), Math.Max(other.Start.X, other.End.X));

                Vector2 newStart = Start.X < End.X ? Start : End;
                Vector2 newEnd = other.Start.X < other.End.X ? other.Start : other.End;

                newStart = (newStart.X < newEnd.X) ? newEnd : newStart;
                newEnd = (newStart.X > newEnd.X) ? newStart : newEnd;

                return new LineSegment(new Vector2(minX, newStart.Y), new Vector2(maxX, newEnd.Y));
            }
            return null;
        }
    }
    public struct Projection
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public Projection(float min, float max)
        {
            Min = min;
            Max = max;
        }
        public readonly float GetOverlap(Projection other)
        {
            if (Intersects(other))
            {
                return Math.Min(Max, other.Max) - Math.Max(Min, other.Min);
            }
            return 0;
        }
        public readonly bool Intersects(Projection other) => !(other.Max < Min || other.Min > Max); //(Max >= other.Min && other.Max >= Min);
    }
    public struct Polygon
    {
        public Vector2 position;
        private float _rotationAngle = 0;
        private readonly List<Vector2> _originalVertices;
        private readonly Vector2 _center = Vector2.Zero;

        public readonly Vector2 Center => _center;

        public readonly Vector2 IntegerPosition => new((int)Math.Floor(position.X), (int)Math.Floor(position.Y));
        public List<Vector2> Vertices { get; private set; }
        public float Rotation
        {
            readonly get => _rotationAngle;
            set
            {
                float oldRotation = _rotationAngle;
                _rotationAngle = value % 360;

                if (oldRotation == _rotationAngle)
                    return;

                if (_rotationAngle < 0)
                    _rotationAngle += 360;

                UpdateVertices();
            }
        }

        public Polygon(Vector2 position, List<Vector2> vertices)
        {
            this.position = position;
            Vertices = vertices;
            _originalVertices = new List<Vector2>(vertices);
            _center = GetCenter();
        }
        public Polygon(List<Vector2> vertices) : this(Vector2.Zero, vertices) { }
        private readonly void UpdateVertices()
        {
            Vertices.Clear();

            foreach (var item in _originalVertices)
                Vertices.Add(RotatePoint(item, _center, _rotationAngle));
        }

        public readonly bool IntersectsWith(Polygon other)
        {
            foreach (var axis in GetAxes().Concat(other.GetAxes()))
            {
                Projection proj1 = GetProjection(axis);
                Projection proj2 = other.GetProjection(axis);

                if (!proj1.Intersects(proj2))
                    return false;
            }

            return true;
        }
        public readonly bool IntersectsWith(Polygon other, out Vector2 mtv)
        {
            mtv = Vector2.Zero;
            float minOverlap = float.MaxValue;
            Vector2 smallestAxis = Vector2.Zero;

            foreach (var axis in GetAxes().Concat(other.GetAxes()))
            {
                Projection proj1 = GetProjection(axis);
                Projection proj2 = other.GetProjection(axis);

                if (!proj1.Intersects(proj2))
                    return false;

                float overlap = proj1.GetOverlap(proj2);
                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axis;
                }
            }

            mtv = smallestAxis * minOverlap * 1f;
            return true;
        }

        public readonly Vector2 GetCenter()
        {
            float x = 0, y = 0;

            foreach (var vertex in _originalVertices)
            {
                x += vertex.X;
                y += vertex.Y;
            }

            float devidedX = x / Vertices.Count;
            float devidedY = y / Vertices.Count;

            return new Vector2(devidedX, devidedY);
        }
        public readonly Projection GetProjection(Vector2 axis)
        {
            float min = Vector2.Dot(axis, Vertices[0] + IntegerPosition);
            float max = min;

            foreach (var vertex in Vertices)
            {
                float projection = Vector2.Dot(axis, vertex + IntegerPosition);
                if (projection < min)
                    min = projection;
                if (projection > max)
                    max = projection;
            }

            return new(min, max);
        }
        public readonly List<Vector2> GetAxes()
        {
            List<Vector2> axes = new();
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vector2 p1 = Vertices[i];
                Vector2 p2 = Vertices[(i + 1) % Vertices.Count];
                Vector2 edge = p2 - p1;

                axes.Add(Vector2.Normalize(new Vector2(-edge.Y, edge.X)));
            }
            return axes;
        }
        public readonly List<LineSegment> GetEdges()
        {
            List<LineSegment> axes = new();
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vector2 p1 = Vertices[i];
                Vector2 p2 = Vertices[(i + 1) % Vertices.Count];
                Vector2 edge = p2 - p1;
                axes.Add(new(p1 + IntegerPosition, p2 + IntegerPosition));
            }
            return axes;
        }
        public static Vector2 RotatePoint(Vector2 point, Vector2 origin, float rotation)
        {
            rotation = Deg2Rad(rotation);

            float cos = (float)Math.Cos(rotation);
            float sin = (float)Math.Sin(rotation);

            Vector2 translatedPoint = point - origin;

            float newX = translatedPoint.X * cos - translatedPoint.Y * sin;
            float newY = translatedPoint.X * sin + translatedPoint.Y * cos;

            return new Vector2(newX, newY) + origin;
        }
    }

    public static class PolygonFactory
    {
        public static List<Vector2> RectangleVerts(float width, float height)
        {
            width /= 2;
            height /= 2;

            return new()
            {
                new(-width, -height),
                new(width, -height),
                new(width, height),
                new(-width, height)
            };
        }
        public static List<Vector2> RightTriangleVerts(float width, float height)
        {
            return new()
            {
                new((int)(-width / 3), (int)height / 3),
                new((int)width, (int)height / 3),
                new((int)-width / 3, (int)-height),
            };
        }

        public static Polygon Rectangle(float width, float height) => new(RectangleVerts(width, height));
        public static Polygon RightTriangle(float width, float height) => new(RightTriangleVerts(width, height));
    }

    public abstract class GameObject
    {
        public Vector2 position = new(0, 0);
        public float rotation = 0;
        public Color color = Color.White;

        public IDrawer drawer;
        public SpriteBatch spriteBatch;
        public Vector2 viewport;

        protected List<ObjectModule> modules = new();

        protected readonly Action<GameTime> drawAction;

        public Vector2 IntegerPosition => new((int)Math.Round(position.X), (int)Math.Round(position.Y));
        public IReadOnlyList<ObjectModule> Modules => modules.ToArray();

        public GameObject(IDrawer drawer)
        {
            this.drawer = drawer;
            spriteBatch = drawer.SpriteBatch;
            drawAction = Draw;

            drawer.AddDrawAction(drawAction);
            viewport = new(spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);
        }
        public virtual void Destroy() => drawer.RemoveDrawAction(drawAction);

        protected abstract void Draw(GameTime gameTime);

        public T AddModule<T>() where T : ObjectModule
        {
            var module = (T)Activator.CreateInstance(typeof(T), args: this);

            if (!ContainsModule<T>())
                modules.Add(module);
            else
                modules[modules.IndexOf(module)] = module;

            return module;
        }
        public void AddModule<T>(T module)
            where T : ObjectModule => modules.Add(module);
        public bool RemoveModule<T>() 
            where T : ObjectModule => RemoveModule(modules.OfType<T>().FirstOrDefault());
        public bool RemoveModule<T>(T module) where T : ObjectModule 
        {
            bool res = modules.Remove(module);
            module.OnRemove();
            return res;
        }
        public T GetModule<T>() 
            where T : ObjectModule => Modules.OfType<T>().FirstOrDefault();
        public ObjectModule[] GetModulesOf<T>() 
            where T : ObjectModule => Modules.OfType<T>().ToArray();
        public bool TryGetModule<T>(out T module)
            where T : ObjectModule => (module = Modules.OfType<T>().FirstOrDefault()) != null;
        public bool ContainsModule<T>()
            where T : ObjectModule => Modules.OfType<T>().Any();
        public bool ContainsModule<T>(T module)
           where T : ObjectModule => Modules.Where(m => m == module).Any();
    }
    public abstract class ObjectModule
    {
        private GameObject _owner;
        public ObjectModule(GameObject owner) => this.Owner = owner;
        public GameObject Owner { get => _owner; set => _owner = value ?? _owner; }
        public abstract void OnRemove();
    }
    
    public class TextObject : GameObject
    {
        public string sprite;
        public SpriteFont font;
        public Vector2 size = Vector2.One;
        public Vector2 center;

        public TextObject(IDrawer drawer, string sprite, SpriteFont font) : base(drawer)
        {
            center = font.MeasureString(sprite) / 2;

            this.sprite = sprite;
            this.font = font;
        }
        protected override void Draw(GameTime gameTime)
        {
            bool canDraw = position.Y >= 0 && position.Y <= viewport.Y && position.X >= 0 && position.X <= viewport.X;
            
            if (canDraw)
                spriteBatch.DrawString(font, sprite, IntegerPosition, color, Deg2Rad(rotation), center, size, SpriteEffects.None, 0);
        }
    }

    public class Collider : ObjectModule
    {
        public Polygon polygon;
        public Color drawColor = Color.Green;
       
        public event Action<Collider> OnTouchEnter;
        public event Action<Collider> OnTouchStay;
        public event Action<Collider> OnTouchExit;

        public event Action<Collider> OnTriggerEnter;
        public event Action<Collider> OnTriggerStay;
        public event Action<Collider> OnTriggerExit;
        public event Action<GameTime> OnCheckFinish;

        private readonly List<Collider> _intersections = new();
        private List<Collider> _lastFrameIntersections = new();

        private ColliderMode _colliderMode;
        private Action _intersectionUpdater;
        private readonly Action<GameTime> _drawAction;
        private readonly ShapeDrawer _shapeDrawer;
        private readonly static List<Collider> _allColliders = new();
        
        public ColliderMode Mode { get => _colliderMode; set => SetUpdater(value); }
        public string Info { get; set; } = "";
        public Rectangle ShapeBounding { get; private set; }
        public bool Intersects { get; private set; }
        public static IReadOnlyList<Collider> AllColliders => _allColliders;
        public IReadOnlyList<Collider> Intersections => _intersections;
        public Collider(GameObject owner) : base(owner)
        {
            Owner = owner;
            _drawAction = Draw;

            polygon = new(owner.position, PolygonFactory.RectangleVerts(50, 50));
            _shapeDrawer = new(IngameDrawer.Instance.GraphicsDevice, IngameDrawer.Instance.SpriteBatch);

            IngameDrawer.Instance.AddDrawAction(_drawAction);
            
            ShapeBounding = GetShapeBounding();

            _allColliders.Add(this);
            GameEvents.Update += Update;

            SetUpdater(ColliderMode.Physical);
        }

        private void Update(GameTime gt)
        {
            _intersections.Clear();
            polygon.position = Owner.position;
            polygon.Rotation = Owner.rotation;
            ShapeBounding = GetShapeBounding();

            _intersectionUpdater();
            OnCheckFinish?.Invoke(gt);

            _lastFrameIntersections = new List<Collider>(_intersections);
            
            Intersects = _intersections.Any();
            drawColor = Intersects ? Color.Red : Color.Green;

        }
        
        private void PhysicalCheck()
        {
            foreach (var item in AllColliders.Where(c => c.Mode == ColliderMode.Physical || c.Mode == ColliderMode.Static))
            {
                if (item == this)
                    continue;

                TextObject otherObj = item.Owner as TextObject;

                if (polygon.IntersectsWith(item.polygon, out var mtv))
                {
                    _intersections.Add(item);

                    PushOut(item, mtv);

                    if (!_lastFrameIntersections.Contains(item))
                        OnTouchEnter?.Invoke(item);
                    else
                        OnTouchStay?.Invoke(item);
                }
                else if (_lastFrameIntersections.Contains(item))
                    OnTouchExit?.Invoke(item);
            }
        }
        private void StatiCheck()
        {
            foreach (var item in AllColliders.Where(c => c.Mode == ColliderMode.Physical))
            {
                if (item == this)
                    continue;

                TextObject otherObj = item.Owner as TextObject;

                if (polygon.IntersectsWith(item.polygon))
                {
                    _intersections.Add(item);

                    if (!_lastFrameIntersections.Contains(item))
                        OnTouchEnter?.Invoke(item);
                    else
                        OnTouchStay?.Invoke(item);
                }
                else if (_lastFrameIntersections.Contains(item))
                    OnTouchExit?.Invoke(item);
            }
        }
        private void TriggerCheck()
        {
            foreach (var item in AllColliders)
            {
                if (item == this)
                    continue;

                TextObject otherObj = item.Owner as TextObject;

                if (polygon.IntersectsWith(item.polygon))
                {
                    _intersections.Add(item);

                    if (!_lastFrameIntersections.Contains(item))
                        OnTriggerExit?.Invoke(item);
                    else
                        OnTriggerExit?.Invoke(item);
                }
                else if (_lastFrameIntersections.Contains(item))
                    OnTriggerExit?.Invoke(item);
            }
        }

        public void PushOut(Collider other, Vector2 mtv)
        {
            Vector2 direction = Owner.IntegerPosition - other.Owner.IntegerPosition;
            
            if (Vector2.Dot(direction, mtv) < 0)
                mtv = -mtv;

            Vector2 displacement;

            if (Math.Abs(mtv.X) > Math.Abs(mtv.Y))
                displacement = new Vector2(mtv.X, 0);
            else
                displacement = new Vector2(0, mtv.Y);


            if (other.Mode == ColliderMode.Physical)
                other.Owner.position -= displacement;
            else if (other.Mode == ColliderMode.Static)
                Owner.position += displacement;
        }

        public Rectangle GetShapeBounding()
        {
            List<Vector2> vertices = polygon.Vertices;

            int width = (int)Math.Ceiling(vertices.Max(v => v.X)) - (int)Math.Floor(vertices.Min(v => v.X));
            int height = (int)Math.Ceiling(vertices.Max(v => v.Y)) - (int)Math.Floor(vertices.Min(v => v.Y));

            return new Rectangle(
                (int)Owner.position.X - width / 2 ,
                (int)Owner.position.Y - height / 2,
                width,
                height);
        }

        private void SetUpdater(ColliderMode newMode)
        {
            _colliderMode = newMode;
            switch (_colliderMode)
            {
                case ColliderMode.Physical:
                    _intersectionUpdater = PhysicalCheck;
                    break;
                case ColliderMode.Trigger:
                    _intersectionUpdater = TriggerCheck;
                    break;
                case ColliderMode.Static:
                    _intersectionUpdater = StatiCheck;
                    break;
            }
        }
        public override void OnRemove()
        {
            _allColliders.Remove(this);
            GameEvents.Update -= Update;
            IngameDrawer.Instance.RemoveDrawAction(_drawAction);
        }
        public void Draw(GameTime gt)
        {
            List<Vector2> vertices = polygon.Vertices;
            Vector2 current = vertices[0] + Owner.position;

            for (int i = 1; i < vertices.Count; i++)
            {
                Vector2 next = vertices[i] + Owner.position;
                _shapeDrawer.DrawLine(current, next, drawColor);
                current = next;
            }

            _shapeDrawer.DrawLine(current, vertices[0] + Owner.position, drawColor);
        }
    }
    public class Rigidbody : ObjectModule
    {
        public float mass = 1;
        public float gravityScale = 1;
        public float bounciness = 0.2f;
        public float friction = 0.3f;
        public float airFriction = 0.3f;

        public float angle = 0;
        public float torque = 0;
        public float angularVelocity = 0;
        public float angularAcceleration = 0;
        public float momentOfInertia = 1;

        public Vector2 forces = Vector2.Zero;
        public Vector2 velocity = Vector2.Zero;
        public Collider collider;

        public const float Gravity = 9.81f;
        public const float FixedDelta = 1.0f / 180.0f;
        private float accumulatedTime = 0.0f;

        public Rigidbody(GameObject owner) : base(owner)
        {
            if (owner.TryGetModule<Collider>(out var module))
                collider = module;
            else
                collider = owner.AddModule<Collider>();


            collider.OnCheckFinish += Update;
        }
        public void AddForce(Vector2 force) => forces += force;
       
        private void Update(GameTime gameTime)
        {
            accumulatedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            while (accumulatedTime >= FixedDelta)
            {
                //ApplyGravity();

                velocity += forces / mass * FixedDelta;
                forces = Vector2.Zero;

                foreach (var item in collider.Intersections)
                {
                    if (item.Owner.TryGetModule(out Rigidbody otherRb))
                        HandleRigidbodyCollision(otherRb);
                    else
                        HandleStaticCollision(item);
                }
                
                ApplyFriction(airFriction);

                Owner.position += velocity;
                accumulatedTime -= FixedDelta;
            }
        }

        
        private void HandleRigidbodyCollision(Rigidbody other)
        {
            List<Point> touchPoints = GetTouchPoints(collider, other.collider);
            if (touchPoints.Count < 1)
                return;
            Vector2 averageTouch = Vector2.Zero;

            foreach (var  p in touchPoints)
            {
                averageTouch += p.ToVector2();



                /*Vector2 r1 = p - collider.Owner.position;
                Vector2 r2 = p - other.collider.Owner.position;


                float angularImpulse1 = Cross(r1, impulse) / this.momentOfInertia;
                float angularImpulse2 = Cross(r2, impulse) / other.momentOfInertia;

                angularVelocity -= angularImpulse1;
                other.angularVelocity += angularImpulse2;

                torque = angularImpulse1 * this.momentOfInertia;
                other.torque = angularImpulse2 * other.momentOfInertia;*/
            }

            averageTouch /= touchPoints.Count;

            Vector2 touchNormal = other.Owner.position - averageTouch;
            touchNormal.Normalize();

            float velocityAlongNormal = Vector2.Dot(other.velocity, touchNormal);
            if (velocityAlongNormal > 0)
                return;

            float e = Math.Min(bounciness, other.bounciness);
            float j = -(1 + e) * velocityAlongNormal;
            j /= 1 / mass + 1 / other.mass;

            Vector2 impulse = j * touchNormal;

            velocity -= impulse / mass;
            other.velocity += impulse / other.mass;
        }
        static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
        private void HandleStaticCollision(Collider other)
        {
            
            List<Point> touchPoints = GetTouchPoints(collider, other);
            if (touchPoints.Count < 1)
                return;

            Vector2 averageTouch = Vector2.Zero;
            foreach (var p in touchPoints)
                averageTouch += p.ToVector2();
            averageTouch /= touchPoints.Count;


            Vector2 touchNormal = Owner.position - averageTouch;
            touchNormal.Normalize();

            float velocityAlongNormal = Vector2.Dot(velocity, touchNormal);
            if (velocityAlongNormal > 0)
                return;

            float e = bounciness;
            float j = -(1 + e) * velocityAlongNormal;
            j /= 1 / mass;


            Vector2 impulse = j * touchNormal;
            velocity += impulse / mass;
        }

        public static List<Point> GetTouchPoints(Collider coll1, Collider coll2)
        {
            LineSegment[] s1 = coll1.polygon.GetEdges().ToArray();
            LineSegment[] s2 = coll2.polygon.GetEdges().ToArray();
            
            Vector2[] v1 = coll1.polygon.Vertices.Select(v => v + coll1.polygon.IntegerPosition).ToArray();
            Vector2[] v2 = coll2.polygon.Vertices.Select(v => v + coll2.polygon.IntegerPosition).ToArray();


            List<Point> touchPoints = new();

            touchPoints = touchPoints.Concat(GetVertsOnSegments(s1, v2)).ToList();
            touchPoints = touchPoints.Concat(GetVertsOnSegments(s2, v1)).ToList();

            return touchPoints.Distinct().ToList();
        }
        public static List<Point> GetVertsOnSegments(LineSegment[] segments, Vector2[] vertices)
        {
            List<Point> points = new();

            foreach (var s in segments)
            {
                foreach (var v in vertices)
                {
                    Point p = v.ToPoint();

                    if (s.IsPointOn(p))
                    {
                        points.Add(p);
                    }
                }
            }
            return points;
        }

        private void ApplyGravity() => AddForce(new(0, (Gravity * mass) * (FixedDelta * 200)));
        private void ApplyFriction(float frictValue)
        {
            float deltaFrict = (frictValue / mass) * FixedDelta;

            if (Math.Abs(velocity.X) > 0)
                velocity.X += velocity.X < 0 ? deltaFrict : -deltaFrict;

            if (Math.Abs(velocity.Y) > 0)
                velocity.Y += velocity.Y < 0 ? deltaFrict : -deltaFrict;
        }

        public override void OnRemove()
        {
            collider.OnCheckFinish -= Update;
        }
    }


    public class OldCollider : ObjectModule
    {
        private readonly static List<OldCollider> colliders = new();

        public Size size = new(50, 50);
        public Point position = new(0, 0);
        public Color drawColor = Color.Green;
        public ColliderMode colliderType = ColliderMode.Trigger;

        private Rectangle _colliderRect = new(0, 0, 0, 0);
        private readonly List<Intersection> _intersectionInfos = new();

        private bool _intersects = false;
        private readonly ShapeDrawer _shapeDrawer = new(IngameDrawer.Instance.GraphicsDevice, IngameDrawer.Instance.SpriteBatch);

        public OldCollider(GameObject owner) : base(owner)
        {
            GameEvents.Update += Update;
            AddCollider(this);

            IngameDrawer.Instance.AddDrawAction((gt) => _shapeDrawer.DrawRectangle(_colliderRect, drawColor));
        }

        public void Update(GameTime gameTime)
        {
            position = new Point((int)Owner.position.X, (int)Owner.position.Y);
            _colliderRect = GetBounds();


            UpdateIntersections();

            drawColor = _intersects ? Color.Red : Color.Green;
        }

        public Rectangle GetBounds() => 
            new(
                position.X - (int)size.Center.X, 
                position.Y - (int)size.Center.Y,
                size.Width, 
                size.Height);
        public void UpdateIntersections()
        {
            _intersectionInfos.Clear();

            foreach (OldCollider coll in AllColliders)
            {
                Point difference = position - coll.position;

                if ((Math.Abs(difference.X) > _colliderRect.X + 10
                && Math.Abs(difference.Y) > _colliderRect.Y + 10)
                || coll == this)
                    continue;

                if (IntersectsWith(coll))
                {
                    _intersectionInfos.Add(GetIntersectionInfo(coll));
                    
                    if(colliderType == ColliderMode.Physical)
                    {
                        foreach (var info in IntersectionInfos)
                        {
                            OldCollider itemCollider = info.gameObject.GetModule<OldCollider>();
                            Vector2 depth = GetIntersectionDepth(itemCollider);

                            if (depth.X > -2 || depth.Y > -2)
                                continue;

                            OldCollider ownerCollider = Owner.GetModule<OldCollider>();

                            int heightHalf = ownerCollider.size.Height / 2;
                            int widthHalf = ownerCollider.size.Width / 2;

                            float t = HTime.DeltaTime * 100;

                            Vector2 newPosition = Owner.position;

                            if (info.contactNormal.X == 1)
                                newPosition.X = itemCollider.ColliderRect.Right + widthHalf;
                            else if (info.contactNormal.X == -1)
                                newPosition.X = itemCollider.ColliderRect.Left - widthHalf;

                            if (info.contactNormal.Y == 1)
                                newPosition.Y = itemCollider.ColliderRect.Bottom + heightHalf;
                            else if (info.contactNormal.Y == -1)
                                newPosition.Y = itemCollider.ColliderRect.Top - heightHalf;

                            Owner.position = Vector2.Lerp(Owner.position, newPosition, t);
                        }
                    }   
                }
            };

            _intersects = _intersectionInfos.Any();
        }

        public bool IntersectsWith(OldCollider other) => GetBounds().Intersects(other.GetBounds());
        public Side GetIntersectionSide(OldCollider other)
        {
            Point difference = position - other.position;

            float dx = difference.X / (float)(_colliderRect.Width + other._colliderRect.Width);
            float dy = difference.Y / (float)(_colliderRect.Height + other._colliderRect.Height);

            float absDx = Math.Abs(dx);
            float absDy = Math.Abs(dy);

            if (absDx > absDy) 
                return dx > 0 ? Side.Left : Side.Right;
            else
                return dy > 0 ? Side.Top : Side.Bottom;
        }

        public Intersection GetIntersectionInfo(OldCollider other) => 
            new(
                other.Owner, 
                GetContactNormal(other), 
                GetIntersectionDepth(other));

        public Vector2 GetContactNormal(OldCollider other)
        {
            Rectangle a = ColliderRect;
            Rectangle b = other.ColliderRect;

            float left = a.Left - b.Right;
            float right = b.Left - a.Right;
            float top = a.Top - b.Bottom;
            float bottom = b.Top - a.Bottom;

            float minOverlapX = Math.Min(Math.Abs(left), Math.Abs(right));
            float minOverlapY = Math.Min(Math.Abs(top), Math.Abs(bottom));

            Vector2 contactNormal = Vector2.Zero;

            if (minOverlapX < minOverlapY)
            {
                contactNormal.X = left < right ? -1 : 1;

                if (a.Top >= b.Top && a.Bottom <= b.Bottom)
                    contactNormal.Y = 0;
                else
                    contactNormal.Y = ((float)a.Center.Y - b.Top) / b.Height - 0.5f;
            }
            else
            {
                contactNormal.Y = top < bottom ? -1 : 1;

                if (a.Left >= b.Left && a.Right <= b.Right)
                    contactNormal.X = 0;
                else
                    contactNormal.X = -(((float)a.Center.X - b.Left) / b.Width - 0.5f);
            }

            return contactNormal;
        }
        public Vector2 GetIntersectionDepth(OldCollider other)
        {
            Rectangle rect1 = GetBounds();
            Rectangle rect2 = other.GetBounds();

            float dx;
            if(rect1.Right < rect2.Left)
                dx = rect2.Left - rect1.Right;
            else if(rect2.Right < rect1.Left)
                dx = rect1.Left - rect2.Right;
            else
                dx = -Math.Min(rect1.Right - rect2.Left, rect2.Right - rect1.Left);

            float dy;
            if(rect1.Bottom < rect2.Top)
                dy = rect2.Top - rect1.Bottom;
            else if(rect2.Bottom < rect1.Top)
                dy = rect1.Top - rect2.Bottom;
            else
                dy = -Math.Min(rect1.Bottom - rect2.Top, rect2.Bottom - rect1.Top);

            return new(dx, dy);
        }

        public static bool IsIntersection(Vector2 other) => IsXIntersection(other.Y) || IsYIntersection(other.X);
        public static bool IsXIntersection(float y) => (y > -0.98 && y < 0.98) || y == 0;
        public static bool IsYIntersection(float x) => (x > -0.98 && x < 0.98) || x == 0;

        public static void AddCollider(OldCollider collider) => colliders.Add(collider);
        public static void RemoveCollider(OldCollider collider) => colliders.Remove(collider);

        public override void OnRemove()
        {
            throw new NotImplementedException();
        }

        public static List<OldCollider> AllColliders => colliders;
        public bool Intersects => _intersects;
        public Rectangle ColliderRect => _colliderRect;
        public IReadOnlyList<Intersection> IntersectionInfos => _intersectionInfos;
    }
    public class OldRigidbody : ObjectModule
    {
        public float mass = 1;
        public float gravityScale = 1;
        public float bounciness = 0.2f;
        public float friction = 0.3f;
        public float airFriction = 0.3f;

        public float angle = 0;
        public float torque = 0;
        public float angularVelocity = 0;
        public float angularAcceleration = 0;

        public Vector2 forces = Vector2.Zero; 
        public Vector2 velocity = Vector2.Zero;

        public const float Gravity = 9.81f;
        public OldCollider collider;

        public const float FixedDelta = 1.0f / 180.0f;
        private float accumulatedTime = 0.0f;

        public OldRigidbody(GameObject owner) : base(owner)
        {
            GameEvents.Update += Update;

            if (owner.TryGetModule<OldCollider>(out var collider))
                this.collider = collider;
            else
                this.collider = owner.AddModule<OldCollider>();

            collider.colliderType = ColliderMode.Physical;
        }

        public void AddForce(Vector2 force) => forces += force;

        private void Update(GameTime gameTime)
        {
            accumulatedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            while(accumulatedTime >= FixedDelta)
            {
                ApplyGravity();

                velocity += forces / mass * FixedDelta;
                forces = Vector2.Zero;

                if (collider.Intersects)
                {
                    foreach (var item in collider.IntersectionInfos)
                    {
                        GameObject otherObject = item.gameObject;
                        OldRigidbody otherRb = otherObject.GetModule<OldRigidbody>();

                        if (otherRb != null)
                            HandleRigidbodyCollision(item, otherRb);
                        else
                            HandleStaticCollision(item);

                        ApplyFriction();
                    }
                }

                ApplyAirFriction();

                Owner.position += velocity;
                accumulatedTime -= FixedDelta;
            }
        }

        private void ApplyGravity() => AddForce(new(0, (Gravity * gravityScale * mass) * (FixedDelta * 200)));
        private void ApplyAirFriction()
        {
            float deltaAirFrict = (airFriction / mass) * FixedDelta;

            if (Math.Abs(velocity.X) > 0f)
                velocity.X += velocity.X < 0 ? deltaAirFrict : -deltaAirFrict;

            if (Math.Abs(velocity.Y) > 0f)
                velocity.Y += velocity.Y < 0 ? deltaAirFrict : -deltaAirFrict;
        }
        private void ApplyFriction()
        {
            float deltaFrict = (friction / mass) * FixedDelta;

            if (Math.Abs(velocity.X) > 0)
                velocity.X += velocity.X < 0 ? deltaFrict : -deltaFrict;

            if (Math.Abs(velocity.Y) > 0)
                velocity.Y += velocity.Y < 0 ? deltaFrict : -deltaFrict;
        }

        private void HandleStaticCollision(Intersection intersectionInfo)
        {
            Vector2 contactNormal = intersectionInfo.contactNormal;

            bool cantX = OldCollider.IsXIntersection(contactNormal.Y);
            bool cantY = OldCollider.IsYIntersection(contactNormal.X);

            if (contactNormal.X == 1 && velocity.X < 0 && cantX)
                velocity.X = 0;
            else if (contactNormal.X == -1 && velocity.X > 0 && cantX)
                velocity.X = 0;

            if (contactNormal.Y == 1 && velocity.Y < 0 && cantY)
                velocity.Y = 0;
            else if (contactNormal.Y == -1 && velocity.Y > 0 && cantY)
                velocity.Y = 0;
        }
        private void HandleRigidbodyCollision(Intersection info, OldRigidbody otherRb)
        {
            Vector2 normal = info.contactNormal;
           
            normal = new(-normal.X, -normal.Y);

            normal.X = normal.X > 0 ? 1 : normal.X < 0 ? -1 : 0;
            if (Math.Abs(normal.Y) == 1)
                normal.X = 0;

            Vector2 relativeVelocity = otherRb.velocity - velocity;
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

            if (velocityAlongNormal > 0)
                return;

            float e = (bounciness + otherRb.bounciness) / 2;

            float j = -(1 + e) * velocityAlongNormal;
            j /= 1 / mass + 1 / otherRb.mass;

            Vector2 impulse = j * normal;

            if (Math.Abs(impulse.X) < 0.2f && Math.Abs(impulse.Y) < 0.2f)
            {
                HandleStaticCollision(info);
                return;
            }

            velocity -= impulse / mass;
            otherRb.velocity += impulse / otherRb.mass;
        }

        public override void OnRemove()
        {
            throw new NotImplementedException();
        }
    }
}