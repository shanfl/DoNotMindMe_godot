using Godot;
using System;
using System.Collections.Generic;

public partial class Npc : CharacterBody2D
{
	private enum EnemyState {Patrolling,Chasing,Searching}


	[Export] private NavigationAgent2D _navAgent;
	[Export] private Node2D _patrolPoint;

	[Export] private Node2D _playerScanner;
	[Export] private RayCast2D _ray;

	[Export]private Sprite2D _warning;
	[Export]private AudioStreamPlayer2D _sound;
	[Export]private AnimationPlayer _anim;
	[Export]  private Timer _shootTimer;
	[Export] private Marker2D _shootPos;

	[Export] PackedScene _bulletScene;
 
	private List<Vector2> _wayPoints = [];
	private int _currentWp = 0;

	private EnemyState _state = EnemyState.Patrolling;

	private Player _playerRef;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("npc ready");
		
		SetPhysicsProcess(false);
		CreateWayPoints();
		CallDeferred(MethodName.LateSetup);

		_shootTimer.Timeout += OnShootTimeTimeout;
	}


	private void Shoot()
	{
		var nb = _bulletScene.Instantiate<Bullet>();
		nb.GlobalPosition = _shootPos.GlobalPosition;
		GetTree().Root.AddChild(nb);
		SoundManager.PlayLaser(_sound);
	}

	private void OnShootTimeTimeout()
	{
		if(_state != EnemyState.Chasing) return;
		Shoot();
	}

	private async void LateSetup()
	{
		await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
		SetPhysicsProcess(true);
		_playerRef = GetTree().GetFirstNodeInGroup(Player.GroupName) as Player;
	}

	private float GetFovAngle()
	{
		var dir = GlobalPosition.DirectionTo(_playerRef.GlobalPosition).Normalized();
		var dotP = dir.Dot(Velocity.Normalized());
		if(dotP >= -1.0f & dotP <= 1.0f)
		{
			return Mathf.RadToDeg(Mathf.Acos(dotP));
		}

		return 0.0f;
	}

	private  bool PlayerInFov()
	{
		return  GetFovAngle() < 60.0f;
	}


	 private void RaycastToPlayer()
	{
		if(!IsInstanceValid(_playerRef)) return;

		_playerScanner.LookAt(_playerRef.GlobalPosition);
	}

	private bool PlayerDetected()
	{
		var c = _ray.GetCollider();
		return c != null  && (c is Player);
	}

	private bool CanSeePlayer()
	{
		return PlayerDetected() && PlayerInFov(); 
	}

	private void SetNextWaypoint()
	{
		_currentWp %= _wayPoints.Count;
		_navAgent.TargetPosition = _wayPoints[_currentWp];
		_currentWp++;
	}


	private void CreateWayPoints()
	{
		foreach(Node2D item in _patrolPoint.GetChildren())
		{
			_wayPoints.Add(item.GlobalPosition);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("test"))
		{
			_navAgent.TargetPosition = GetGlobalMousePosition();
		}
		RaycastToPlayer();
		UpdateState();
		UpdateMovement();
		UpdateDebugLabel();
		UpdateNavigation();
	}

    private void UpdateMovement()
    {
		switch (_state)
		{
			case EnemyState.Patrolling:
				ProcessPatrolling();
				break;
			case EnemyState.Chasing:
				ProcessChasing();
				break;
			case EnemyState.Searching:
				ProcessSearching();
				break;
			default:
				break;
		}
    }

    private void ProcessSearching()
    {
		if (_navAgent.IsNavigationFinished())
		{
			SetState(EnemyState.Patrolling);
		}
    }

    private void ProcessChasing()
    {
		_navAgent.TargetPosition = _playerRef.GlobalPosition;
    }

    private void SetState(EnemyState newState)
	{
		if(_state == newState) return;

		if(newState == EnemyState.Patrolling)
		{
			_warning.Hide();
			_anim.Play("RESET");
			SetNextWaypoint();
		} 
		else if(newState == EnemyState.Searching)
		{
			_warning.Show();	
			_anim.Play("RESET");		
		}		
		else if(newState == EnemyState.Chasing)
		{
			_warning.Hide();		
			SoundManager.PlayGasp(_sound);	
			_anim.Play("flash");
		}

		_state = newState;
	}

	private void UpdateState()
	{
		var newState = _state;
		var cansee = CanSeePlayer();

		if (cansee)
		{
			newState = EnemyState.Chasing;
		}
		else if(!cansee && _state == EnemyState.Chasing)
		{
			newState = EnemyState.Searching;		
		}

		SetState(newState);
	}

    private void ProcessPatrolling()
    {
		if (_navAgent.IsNavigationFinished())
		{
			SetNextWaypoint();
		}
    }

    private void UpdateNavigation()
	{
		if (!_navAgent.IsNavigationFinished())
		{
			var npp = _navAgent.GetNextPathPosition();
			LookAt(npp);
			Velocity = Position.DirectionTo(npp) * 60.0f;
			MoveAndSlide();
		}
	}

	private void UpdateDebugLabel()
	{
		string s = "";
		s += $"IsTargetReachable:{_navAgent.IsTargetReachable()}\n";
		s += $"IsTargetReached:{_navAgent.IsTargetReached()}\n";
		s += $"IsNavigationFinished:{_navAgent.IsNavigationFinished()}\n";
		s += $"Target:{_navAgent.TargetPosition}\n";
		s += $"Fov:{GetFovAngle():F2}  PlayerInFov:{PlayerInFov()} \n";
		s += $"CanSeePlayer: {CanSeePlayer()}, state:{_state}\n";
		SignalManager.EmitOnDebugLabel(s);
	}
}
